using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FellowOakDicom;
using FellowOakDicom.Network;
using FellowOakDicom.Printing;
using Microsoft.Extensions.Logging;
using Print_SCP.Models;
using Print_SCP.Rendering;
using Print_SCP.Services;

namespace Print_SCP.Dicom
{
    internal sealed class DicomPrintScp : DicomService, IDicomServiceProvider, IDicomNServiceProvider, IDicomCEchoProvider
    {
        private static readonly DicomTransferSyntax[] AcceptedTransferSyntaxes =
        {
            DicomTransferSyntax.ExplicitVRLittleEndian,
            DicomTransferSyntax.ExplicitVRBigEndian,
            DicomTransferSyntax.ImplicitVRLittleEndian
        };

        private readonly object _sync = new object();
        private FilmSession _filmSession;
        private PrintJobInfo _job;

        public DicomPrintScp(INetworkStream stream, Encoding fallbackEncoding, ILogger log, DicomServiceDependencies dependencies)
            : base(stream, fallbackEncoding, log, dependencies)
        {
        }

        public Task OnReceiveAssociationRequestAsync(DicomAssociation association)
        {
            _job = new PrintJobInfo
            {
                CallingAeTitle = association.CallingAE ?? "",
                CalledAeTitle = association.CalledAE ?? "",
                RemoteIp = association.RemoteHost ?? "",
                Status = "Association accepted",
                Message = "Connected"
            };

            foreach (var pc in association.PresentationContexts)
            {
                if (IsSupported(pc.AbstractSyntax))
                    pc.AcceptTransferSyntaxes(AcceptedTransferSyntaxes);
                else
                    pc.SetResult(DicomPresentationContextResult.RejectAbstractSyntaxNotSupported);
            }

            Log("ASSOCIATE", "ACCEPTED",
                "Calling AE=" + _job.CallingAeTitle + ", Called AE=" + _job.CalledAeTitle);

            PrintEvents.RaiseJob(_job);
            return SendAssociationAcceptAsync(association);
        }

        public Task OnReceiveAssociationReleaseRequestAsync()
        {
            Log("ASSOCIATE", "RELEASED", "Association released.");
            return SendAssociationReleaseResponseAsync();
        }

        public void OnReceiveAbort(DicomAbortSource source, DicomAbortReason reason)
        {
            Log("ASSOCIATE", "ABORTED", source + " / " + reason);
        }

        public void OnConnectionClosed(Exception exception)
        {
            Log("CONNECTION", "CLOSED", exception == null ? "Connection closed." : exception.Message);
        }

        public Task<DicomCEchoResponse> OnCEchoRequestAsync(DicomCEchoRequest request)
        {
            Log("C-ECHO", "SUCCESS", "Verification request.");
            return Task.FromResult(new DicomCEchoResponse(request, DicomStatus.Success));
        }

        public Task<DicomNCreateResponse> OnNCreateRequestAsync(DicomNCreateRequest request)
        {
            lock (_sync)
            {
                if (request.SOPClassUID == DicomUID.BasicFilmSession)
                {
                    var isColor = request.PresentationContext != null &&
                                  request.PresentationContext.AbstractSyntax == DicomUID.BasicColorPrintManagementMeta;

                    _filmSession = new FilmSession(
                        request.SOPClassUID,
                        request.SOPInstanceUID,
                        request.Dataset,
                        isColor);

                    _job.SopClassUid = request.SOPClassUID.UID;
                    _job.SopInstanceUid = _filmSession.SOPInstanceUID.UID;
                    _job.Status = "Film session created";
                    _job.Message = isColor ? "Color print session" : "Grayscale print session";

                    if (string.IsNullOrEmpty(request.SOPInstanceUID?.UID))
                        request.Command.AddOrUpdate(DicomTag.AffectedSOPInstanceUID, _filmSession.SOPInstanceUID);

                    Log("N-CREATE", "SUCCESS", "Basic Film Session created.");
                    PrintEvents.RaiseJob(_job);
                    return Task.FromResult(new DicomNCreateResponse(request, DicomStatus.Success));
                }

                if (request.SOPClassUID == DicomUID.BasicFilmBox)
                {
                    if (_filmSession == null)
                        return Task.FromResult(new DicomNCreateResponse(request, DicomStatus.NoSuchObjectInstance));

                    var filmBox = _filmSession.CreateFilmBox(request.SOPInstanceUID, request.Dataset);
                    if (!InitializeFilmBoxCompat(filmBox, _filmSession.IsColor))
                        return Task.FromResult(new DicomNCreateResponse(request, DicomStatus.ProcessingFailure));

                    UpdateFilmInfo(filmBox);
                    _job.Status = "Film box created";
                    _job.Message = _job.FilmSizeId + " / " + _job.Layout;

                    if (string.IsNullOrEmpty(request.SOPInstanceUID?.UID))
                        request.Command.AddOrUpdate(DicomTag.AffectedSOPInstanceUID, filmBox.SOPInstanceUID);

                    Log("N-CREATE", "SUCCESS", "Basic Film Box: " + _job.Message);
                    PrintEvents.RaiseJob(_job);

                    return Task.FromResult(new DicomNCreateResponse(request, DicomStatus.Success)
                    {
                        Dataset = filmBox
                    });
                }

                return Task.FromResult(new DicomNCreateResponse(request, DicomStatus.SOPClassNotSupported));
            }
        }

        public Task<DicomNSetResponse> OnNSetRequestAsync(DicomNSetRequest request)
        {
            lock (_sync)
            {
                if (_filmSession == null)
                    return Task.FromResult(new DicomNSetResponse(request, DicomStatus.NoSuchObjectInstance));

                if (request.SOPClassUID == DicomUID.BasicGrayscaleImageBox ||
                    request.SOPClassUID == DicomUID.BasicColorImageBox)
                {
                    var imageBox = _filmSession.FindImageBox(request.SOPInstanceUID);
                    if (imageBox == null)
                        return Task.FromResult(new DicomNSetResponse(request, DicomStatus.NoSuchObjectInstance));

                    if (request.Dataset != null)
                        request.Dataset.CopyTo(imageBox);

                    _job.ReceivedImages++;
                    _job.Status = "Receiving images";
                    _job.Message = _job.ReceivedImages + " / " + Math.Max(_job.TotalImages, _job.ReceivedImages);

                    Log("N-SET", "SUCCESS", "Image Box " + _job.Message);
                    PrintEvents.RaiseJob(_job);
                    return Task.FromResult(new DicomNSetResponse(request, DicomStatus.Success));
                }

                if (request.SOPClassUID == DicomUID.BasicFilmBox)
                {
                    var filmBox = _filmSession.FindFilmBox(request.SOPInstanceUID);
                    if (filmBox == null)
                        return Task.FromResult(new DicomNSetResponse(request, DicomStatus.NoSuchObjectInstance));

                    if (request.Dataset != null)
                        request.Dataset.CopyTo(filmBox);

                    InitializeFilmBoxCompat(filmBox, _filmSession.IsColor);
                    UpdateFilmInfo(filmBox);
                    PrintEvents.RaiseJob(_job);

                    return Task.FromResult(new DicomNSetResponse(request, DicomStatus.Success)
                    {
                        Dataset = filmBox
                    });
                }

                if (request.SOPClassUID == DicomUID.BasicFilmSession)
                {
                    if (request.Dataset != null)
                        request.Dataset.CopyTo(_filmSession);

                    return Task.FromResult(new DicomNSetResponse(request, DicomStatus.Success));
                }

                return Task.FromResult(new DicomNSetResponse(request, DicomStatus.SOPClassNotSupported));
            }
        }

        public Task<DicomNActionResponse> OnNActionRequestAsync(DicomNActionRequest request)
        {
            FilmSession session;
            List<FilmBox> filmBoxes;
            PrintJobInfo job;

            lock (_sync)
            {
                if (_filmSession == null)
                    return Task.FromResult(new DicomNActionResponse(request, DicomStatus.InvalidObjectInstance));

                if (request.ActionTypeID != 0x0001)
                    return Task.FromResult(new DicomNActionResponse(request, DicomStatus.NoSuchActionType));

                if (request.SOPClassUID == DicomUID.BasicFilmSession)
                {
                    filmBoxes = _filmSession.BasicFilmBoxes.ToList();
                }
                else if (request.SOPClassUID == DicomUID.BasicFilmBox)
                {
                    var filmBox = _filmSession.FindFilmBox(request.SOPInstanceUID);
                    if (filmBox == null)
                        return Task.FromResult(new DicomNActionResponse(request, DicomStatus.NoSuchObjectInstance));

                    filmBoxes = new List<FilmBox> { filmBox };
                }
                else
                {
                    return Task.FromResult(new DicomNActionResponse(request, DicomStatus.NoSuchSOPClass));
                }

                session = _filmSession;
                job = CloneJob(_job);
                _job.Status = "Queued";
                _job.Message = "Print accepted; rendering queued";
                PrintEvents.RaiseJob(_job);
                Log("N-ACTION", "ACCEPTED", "Print accepted and queued.");
            }

            Task.Run(() => ProcessPrintAsync(session, filmBoxes, job));
            return Task.FromResult(new DicomNActionResponse(request, DicomStatus.Success));
        }

        private async Task ProcessPrintAsync(
            FilmSession session,
            List<FilmBox> filmBoxes,
            PrintJobInfo job)
        {
            var rendered = new List<RenderedPrintInfo>();
            var settings = PrintRuntime.Settings;

            try
            {
                job.Status = "Rendering";
                job.Message = "Creating final film image";
                PrintEvents.RaiseJob(job);
                LogFor(job, "RENDER", "STARTED", "Rendering " + filmBoxes.Count + " film box(es).");

                foreach (var filmBox in filmBoxes)
                {
                    UpdateFilmInfo(job, filmBox);
                    var item = FilmRenderer.Render(
                        filmBox,
                        job,
                        settings.RenderDpi,
                        settings.OutputRoot);

                    rendered.Add(item);
                }

                job.Status = "Rendered";
                job.Message = rendered.Count + " film(s) rendered";
                PrintEvents.RaiseJob(job);
                LogFor(job, "RENDER", "SUCCESS", job.Message);

                if (settings.WindowsPrintEnabled)
                {
                    job.Status = "Windows printing";
                    job.Message = settings.WindowsPrinterName;
                    PrintEvents.RaiseJob(job);

                    foreach (var item in rendered)
                    {
                        try
                        {
                            await Task.Run(() =>
                                WindowsPrintService.Print(item, settings.WindowsPrinterName));
                            item.WindowsPrintStatus = "Success";
                        }
                        catch (Exception ex)
                        {
                            item.WindowsPrintStatus = "Failed";
                            item.ErrorMessage = AppendError(item.ErrorMessage, "Windows: " + ex.Message);
                            LogFor(job, "WINDOWS PRINT", "FAILED", ex.Message);
                        }
                    }

                    LogFor(job, "WINDOWS PRINT", "FINISHED", settings.WindowsPrinterName);
                }

                if (settings.DicomForwardEnabled)
                {
                    job.Status = "DICOM forwarding";
                    job.Message = settings.DicomRemoteHost + ":" + settings.DicomRemotePort;
                    PrintEvents.RaiseJob(job);

                    try
                    {
                        await DicomPrintForwarder.ForwardAsync(
                            session,
                            filmBoxes,
                            settings.DicomRemoteHost,
                            settings.DicomRemotePort,
                            settings.DicomCallingAe,
                            settings.DicomCalledAe);

                        foreach (var item in rendered)
                            item.DicomForwardStatus = "Success";

                        LogFor(job, "DICOM FORWARD", "SUCCESS",
                            settings.DicomCalledAe + " @ " + settings.DicomRemoteHost);
                    }
                    catch (Exception ex)
                    {
                        foreach (var item in rendered)
                        {
                            item.DicomForwardStatus = "Failed";
                            item.ErrorMessage = AppendError(item.ErrorMessage, "DICOM: " + ex.Message);
                        }

                        LogFor(job, "DICOM FORWARD", "FAILED", ex.Message);
                    }
                }

                job.Status = "Completed";
                job.Message = rendered.Count + " film(s) completed";
                PrintEvents.RaiseJob(job);

                foreach (var item in rendered)
                {
                    PrintRuntime.Archive.Save(item);

                    if (item.Image != null)
                    {
                        item.Image.Dispose();
                        item.Image = null;
                    }

                    PrintEvents.RaiseCompleted(item);
                }

                LogFor(job, "PRINT", "COMPLETED", job.Message);
            }
            catch (Exception ex)
            {
                job.Status = "Failed";
                job.Message = ex.Message;
                PrintEvents.RaiseJob(job);
                LogFor(job, "PRINT", "FAILED", ex.ToString());
            }
        }

        private static string AppendError(string current, string value)
        {
            if (string.IsNullOrWhiteSpace(current)) return value;
            return current + " | " + value;
        }

        private static PrintJobInfo CloneJob(PrintJobInfo source)
        {
            return new PrintJobInfo
            {
                JobId = source.JobId,
                CreatedAt = source.CreatedAt,
                CallingAeTitle = source.CallingAeTitle,
                CalledAeTitle = source.CalledAeTitle,
                RemoteIp = source.RemoteIp,
                SopClassUid = source.SopClassUid,
                SopInstanceUid = source.SopInstanceUid,
                FilmSizeId = source.FilmSizeId,
                Orientation = source.Orientation,
                Layout = source.Layout,
                ReceivedImages = source.ReceivedImages,
                TotalImages = source.TotalImages,
                Status = source.Status,
                Message = source.Message
            };
        }

        public Task<DicomNDeleteResponse> OnNDeleteRequestAsync(DicomNDeleteRequest request)
        {
            lock (_sync)
            {
                if (_filmSession == null)
                    return Task.FromResult(new DicomNDeleteResponse(request, DicomStatus.NoSuchObjectInstance));

                if (request.SOPClassUID == DicomUID.BasicFilmBox)
                {
                    var ok = _filmSession.DeleteFilmBox(request.SOPInstanceUID);
                    return Task.FromResult(new DicomNDeleteResponse(
                        request, ok ? DicomStatus.Success : DicomStatus.NoSuchObjectInstance));
                }

                if (request.SOPClassUID == DicomUID.BasicFilmSession)
                {
                    _filmSession = null;
                    return Task.FromResult(new DicomNDeleteResponse(request, DicomStatus.Success));
                }

                return Task.FromResult(new DicomNDeleteResponse(request, DicomStatus.NoSuchSOPClass));
            }
        }

        public Task<DicomNGetResponse> OnNGetRequestAsync(DicomNGetRequest request)
        {
            if (request.SOPClassUID == DicomUID.Printer &&
                request.SOPInstanceUID == DicomUID.PrinterInstance)
            {
                var ds = new DicomDataset
                {
                    { DicomTag.PrinterStatus, "NORMAL" },
                    { DicomTag.PrinterStatusInfo, "NORMAL" },
                    { DicomTag.PrinterName, "Mehrai Virtual DICOM Printer" },
                    { DicomTag.Manufacturer, "Mehrai Medical" },
                    { DicomTag.ManufacturerModelName, "Print_SCP" },
                    { DicomTag.SoftwareVersions, "1.0" }
                };

                return Task.FromResult(new DicomNGetResponse(request, DicomStatus.Success)
                {
                    Dataset = ds
                });
            }

            return Task.FromResult(new DicomNGetResponse(request, DicomStatus.NoSuchSOPClass));
        }

        public Task<DicomNEventReportResponse> OnNEventReportRequestAsync(DicomNEventReportRequest request)
        {
            return Task.FromResult(new DicomNEventReportResponse(request, DicomStatus.Success));
        }

        private bool InitializeFilmBoxCompat(FilmBox filmBox, bool isColor)
        {
            if (filmBox.Initialize())
                return true;

            string format = (filmBox.ImageDisplayFormat ?? "").Trim().ToUpperInvariant();
            int count = 0;

            if (format == "SLIDE")
                count = ResolveLegacySlideCount(filmBox.FilmSizeID, false);
            else if (format == "SUPERSLIDE")
                count = ResolveLegacySlideCount(filmBox.FilmSizeID, true);
            else if (format.StartsWith("CUSTOM\\", StringComparison.OrdinalIgnoreCase))
                count = 1;

            if (count <= 0)
                return false;

            filmBox.BasicImageBoxes.Clear();
            var referenced = new DicomSequence(DicomTag.ReferencedImageBoxSequence);
            filmBox.AddOrUpdate(referenced);

            DicomUID imageClass = isColor
                ? DicomUID.BasicColorImageBox
                : DicomUID.BasicGrayscaleImageBox;

            for (int i = 0; i < count; i++)
            {
                var sopInstance = new DicomUID(
                    filmBox.SOPInstanceUID.UID + "." + (i + 1),
                    filmBox.SOPInstanceUID.Name,
                    filmBox.SOPInstanceUID.Type);

                var imageBox = new ImageBox(filmBox, imageClass, sopInstance)
                {
                    ImageBoxPosition = (ushort)(i + 1)
                };

                filmBox.BasicImageBoxes.Add(imageBox);
                referenced.Items.Add(new DicomDataset
                {
                    { DicomTag.ReferencedSOPClassUID, imageClass },
                    { DicomTag.ReferencedSOPInstanceUID, sopInstance }
                });
            }

            Log("LAYOUT", "COMPAT",
                format + " initialized by compatibility layout with " + count + " image box(es).");

            return true;
        }

        private static int ResolveLegacySlideCount(string filmSizeId, bool superSlide)
        {
            string size = (filmSizeId ?? "").Trim().ToUpperInvariant();

            if (size == "14INX17IN") return superSlide ? 20 : 24;
            if (size == "11INX14IN") return superSlide ? 15 : 20;
            if (size == "10INX12IN") return superSlide ? 12 : 15;
            if (size == "8INX10IN") return superSlide ? 9 : 12;
            if (size == "A3") return superSlide ? 20 : 24;
            if (size == "A4") return superSlide ? 12 : 15;

            return superSlide ? 12 : 15;
        }

        private void UpdateFilmInfo(FilmBox filmBox)
        {
            _job.FilmSizeId = filmBox.FilmSizeID;
            _job.Orientation = filmBox.FilmOrientation;
            _job.Layout = filmBox.ImageDisplayFormat;
            _job.TotalImages = filmBox.BasicImageBoxes == null ? 0 : filmBox.BasicImageBoxes.Count;
        }

        private static void UpdateFilmInfo(PrintJobInfo job, FilmBox filmBox)
        {
            job.FilmSizeId = filmBox.FilmSizeID;
            job.Orientation = filmBox.FilmOrientation;
            job.Layout = filmBox.ImageDisplayFormat;
            job.TotalImages = filmBox.BasicImageBoxes == null ? 0 : filmBox.BasicImageBoxes.Count;
        }

        private static void LogFor(
            PrintJobInfo job, string command, string status, string message)
        {
            PrintEvents.RaiseLog(new PrintLogEntry
            {
                RemoteIp = job.RemoteIp,
                CallingAe = job.CallingAeTitle,
                CalledAe = job.CalledAeTitle,
                Command = command,
                Status = status,
                Message = message
            });
        }

        private static bool IsSupported(DicomUID uid)
        {
            return uid == DicomUID.Verification ||
                   uid == DicomUID.BasicGrayscalePrintManagementMeta ||
                   uid == DicomUID.BasicColorPrintManagementMeta ||
                   uid == DicomUID.Printer ||
                   uid == DicomUID.PrintJob ||
                   uid == DicomUID.BasicFilmSession ||
                   uid == DicomUID.BasicFilmBox ||
                   uid == DicomUID.BasicGrayscaleImageBox ||
                   uid == DicomUID.BasicColorImageBox;
        }

        private void Log(string command, string status, string message)
        {
            PrintEvents.RaiseLog(new PrintLogEntry
            {
                RemoteIp = _job != null ? _job.RemoteIp : (Association != null ? Association.RemoteHost : ""),
                CallingAe = _job != null ? _job.CallingAeTitle : "",
                CalledAe = _job != null ? _job.CalledAeTitle : "",
                Command = command,
                Status = status,
                Message = message
            });
        }
    }
}