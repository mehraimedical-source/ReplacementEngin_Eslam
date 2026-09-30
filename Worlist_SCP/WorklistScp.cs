using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using FellowOakDicom;
using FellowOakDicom.Network;

namespace Worlist_SCP
{
    public class WorklistScp : DicomService, IDicomServiceProvider, IDicomCEchoProvider, IDicomCFindProvider
    {
        private readonly string _associationId = Guid.NewGuid().ToString("N").Substring(0, 8);
        private string _callingAE = "";
        private string _calledAE = "";
        private string _remoteIP = "";

        public WorklistScp(INetworkStream stream, System.Text.Encoding fallbackEncoding,
            Microsoft.Extensions.Logging.ILogger log, DicomServiceDependencies dependencies)
            : base(stream, fallbackEncoding, log, dependencies)
        {
            _remoteIP = stream?.RemoteHost ?? "";

            WorklistEvents.WriteLog(Log(
                "TCP",
                "CONNECTED",
                null,
                null,
                null,
                "TCP client connected" +
                (stream == null ? "" : " from " + stream.RemoteHost + ":" + stream.RemotePort)));

            WorklistEvents.WriteLog(Log(
                "DICOM",
                "WAITING",
                null,
                null,
                null,
                "Waiting for DICOM A-ASSOCIATE-RQ"));
        }

        public Task OnReceiveAssociationRequestAsync(DicomAssociation association)
        {
            _callingAE = association.CallingAE ?? "";
            _calledAE = association.CalledAE ?? "";
            _remoteIP = association.RemoteHost ?? _remoteIP;

            WorklistEvents.WriteLog(Log("ASSOCIATE", "STARTED", null, null, null,
                "Association request received"));

            DebugTrace.WriteSection("ASSOCIATION REQUEST",
                "Remote Host : " + _remoteIP + Environment.NewLine +
                "Calling AE  : " + _callingAE + Environment.NewLine +
                "Called AE   : " + _calledAE + Environment.NewLine +
                "Contexts    : " + association.PresentationContexts.Count);

            // Intentionally do NOT validate Called AE Title.
            foreach (var pc in association.PresentationContexts)
            {
                if (pc.AbstractSyntax == DicomUID.Verification ||
                    pc.AbstractSyntax == DicomUID.ModalityWorklistInformationModelFind)
                {
                    pc.AcceptTransferSyntaxes(
                        DicomTransferSyntax.ExplicitVRLittleEndian,
                        DicomTransferSyntax.ImplicitVRLittleEndian);
                }
                else
                {
                    pc.SetResult(DicomPresentationContextResult.RejectAbstractSyntaxNotSupported);
                }
            }

            WorklistEvents.WriteLog(Log("ASSOCIATE", "ACCEPTED", null, null, null,
                "Called AE accepted: " + _calledAE));
            return SendAssociationAcceptAsync(association);
        }

        public Task OnReceiveAssociationReleaseRequestAsync()
        {
            WorklistEvents.WriteLog(Log("RELEASE", "FINISHED", null, null, null, "Association released"));
            return SendAssociationReleaseResponseAsync();
        }

        public void OnReceiveAbort(DicomAbortSource source, DicomAbortReason reason)
        {
            WorklistEvents.WriteLog(Log("ASSOCIATE", "FAILED", null, null, null,
                "Abort: " + source + " / " + reason));
        }

        public void OnConnectionClosed(Exception exception)
        {
            WorklistEvents.WriteLog(Log("CONNECTION",
                exception == null ? "DISCONNECTED" : "FAILED",
                null, null, null, exception?.Message ?? "Connection closed"));
        }

        public Task<DicomCEchoResponse> OnCEchoRequestAsync(DicomCEchoRequest request)
        {
            WorklistEvents.WriteLog(Log("C-ECHO", "FINISHED", null, null, null, "Echo OK"));
            return Task.FromResult(new DicomCEchoResponse(request, DicomStatus.Success));
        }

        public async IAsyncEnumerable<DicomCFindResponse> OnCFindRequestAsync(DicomCFindRequest request)
        {
            var sw = Stopwatch.StartNew();
            DebugTrace.WriteDataset("C-FIND REQUEST DATASET", request.Dataset);
            var args = BuildEventArgs(request.Dataset);
            WorklistEvents.WriteLog(Log("C-FIND MWL", "STARTED", Summarize(args), null, null,
                "Worklist request received"));

            List<WorklistItem> items = null;
            Exception requestError = null;
            try
            {
                items = await WorklistEvents.RequestAsync(args).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                requestError = ex;
            }

            if (requestError != null)
            {
                WorklistEvents.WriteLog(Log("C-FIND MWL", "FAILED", Summarize(args), 0, sw.ElapsedMilliseconds, requestError.Message));
                yield return new DicomCFindResponse(request, DicomStatus.ProcessingFailure);
                yield break;
            }

            int sent = 0;
            foreach (var item in items)
            {
                sent++;
                WorklistEvents.WriteLog(Log("C-FIND MWL", "PENDING", Summarize(args), sent, null,
                    item.PatientID + " / " + item.PatientName));

                var responseDataset = ToDataset(item);
                DebugTrace.WriteDataset("C-FIND RESPONSE DATASET #" + sent, responseDataset);

                yield return new DicomCFindResponse(request, DicomStatus.Pending)
                {
                    Dataset = responseDataset
                };
            }

            sw.Stop();
            WorklistEvents.WriteLog(Log("C-FIND MWL", "FINISHED", Summarize(args), sent, sw.ElapsedMilliseconds,
                "C-FIND completed"));
            yield return new DicomCFindResponse(request, DicomStatus.Success);
        }

        private WorklistRequestEventArgs BuildEventArgs(DicomDataset query)
        {
            var args = new WorklistRequestEventArgs
            {
                RemoteIP = _remoteIP,
                CallingAETitle = _callingAE,
                CalledAETitle = _calledAE,
                Query = query,
                PatientID = Get(query, DicomTag.PatientID),
                PatientName = Get(query, DicomTag.PatientName),
                AccessionNumber = Get(query, DicomTag.AccessionNumber)
            };

            if (query != null && query.TryGetSequence(DicomTag.ScheduledProcedureStepSequence, out var seq) && seq.Items.Count > 0)
            {
                args.Modality = Get(seq.Items[0], DicomTag.Modality);
                args.ScheduledStationAETitle = Get(seq.Items[0], DicomTag.ScheduledStationAETitle);
            }

            return args;
        }

        private static string Get(DicomDataset ds, DicomTag tag)
        {
            if (ds == null) return "";
            return ds.TryGetString(tag, out var value) ? value ?? "" : "";
        }

        private static string Summarize(WorklistRequestEventArgs e)
        {
            return "Modality=" + (e.Modality ?? "") +
                   "; PatientID=" + (e.PatientID ?? "") +
                   "; PatientName=" + (e.PatientName ?? "");
        }

        private static DicomDataset ToDataset(WorklistItem item)
        {
            var scheduled = new DicomDataset();
            scheduled.AddOrUpdate(DicomTag.ScheduledStationAETitle, item.ScheduledStationAETitle ?? "");
            scheduled.AddOrUpdate(DicomTag.ScheduledProcedureStepStartDate, item.ScheduledDateTime.ToString("yyyyMMdd"));
            scheduled.AddOrUpdate(DicomTag.ScheduledProcedureStepStartTime, item.ScheduledDateTime.ToString("HHmmss"));
            scheduled.AddOrUpdate(DicomTag.Modality, item.Modality ?? "");
            scheduled.AddOrUpdate(DicomTag.ScheduledProcedureStepDescription, item.ProcedureDescription ?? "");
            scheduled.AddOrUpdate(DicomTag.ScheduledProcedureStepID, item.ScheduledProcedureStepID ?? "");

            var ds = new DicomDataset();
            ds.AddOrUpdate(DicomTag.SpecificCharacterSet, "ISO_IR 192");
            ds.AddOrUpdate(DicomTag.PatientName, item.PatientName ?? "");
            ds.AddOrUpdate(DicomTag.PatientID, item.PatientID ?? "");
            ds.AddOrUpdate(DicomTag.PatientSex, item.Sex ?? "");
            ds.AddOrUpdate(DicomTag.PatientBirthDate, item.BirthDate.HasValue ? item.BirthDate.Value.ToString("yyyyMMdd") : "");
            ds.AddOrUpdate(DicomTag.AccessionNumber, item.AccessionNumber ?? "");
            ds.AddOrUpdate(DicomTag.ReferringPhysicianName, item.ReferringPhysicianName ?? "");
            ds.AddOrUpdate(DicomTag.RequestedProcedureID, item.RequestedProcedureID ?? "");
            ds.AddOrUpdate(DicomTag.RequestedProcedureDescription, item.ProcedureDescription ?? "");
            ds.Add(new DicomSequence(DicomTag.ScheduledProcedureStepSequence, scheduled));
            return ds;
        }

        private WorklistLogEntry Log(string command, string status, string query, int? results, long? duration, string message)
        {
            return new WorklistLogEntry
            {
                AssociationId = _associationId,
                RemoteIP = _remoteIP,
                CallingAE = _callingAE,
                CalledAE = _calledAE,
                Command = command,
                Status = status,
                Query = query,
                Results = results,
                DurationMs = duration,
                Message = message
            };
        }
    }
}
