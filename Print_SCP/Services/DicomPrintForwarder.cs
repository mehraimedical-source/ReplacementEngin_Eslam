using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FellowOakDicom;
using FellowOakDicom.Network;
using FellowOakDicom.Network.Client;
using FellowOakDicom.Printing;

namespace Print_SCP.Services
{
    internal static class DicomPrintForwarder
    {
        public static async Task ForwardAsync(
            FilmSession filmSession,
            IEnumerable<FilmBox> filmBoxes,
            string host,
            int port,
            string callingAe,
            string calledAe)
        {
            if (filmSession == null)
                throw new ArgumentNullException(nameof(filmSession));

            var client = DicomClientFactory.Create(
                host,
                port,
                false,
                callingAe,
                calledAe);

            await client.AddRequestAsync(
                new DicomNCreateRequest(
                    filmSession.SOPClassUID,
                    filmSession.SOPInstanceUID)
                {
                    Dataset = filmSession
                });

            foreach (FilmBox filmBox in (filmBoxes ?? filmSession.BasicFilmBoxes).ToList())
            {
                var imageRequests = new List<DicomNSetRequest>();

                var filmBoxRequest = new DicomNCreateRequest(
                    FilmBox.SOPClassUID,
                    filmBox.SOPInstanceUID)
                {
                    Dataset = filmBox
                };

                filmBoxRequest.OnResponseReceived = (request, response) =>
                {
                    if (!response.HasDataset) return;

                    DicomSequence sequence;
                    if (!response.Dataset.TryGetSequence(
                        DicomTag.ReferencedImageBoxSequence,
                        out sequence))
                        return;

                    int count = Math.Min(sequence.Items.Count, imageRequests.Count);
                    for (int i = 0; i < count; i++)
                    {
                        string instanceUid = sequence.Items[i]
                            .GetSingleValue<string>(DicomTag.ReferencedSOPInstanceUID);

                        imageRequests[i].Dataset.AddOrUpdate(
                            DicomTag.SOPInstanceUID,
                            instanceUid);

                        imageRequests[i].Command.AddOrUpdate(
                            DicomTag.RequestedSOPInstanceUID,
                            instanceUid);
                    }
                };

                await client.AddRequestAsync(filmBoxRequest);

                foreach (ImageBox image in filmBox.BasicImageBoxes)
                {
                    var request = new DicomNSetRequest(
                        image.SOPClassUID,
                        image.SOPInstanceUID)
                    {
                        Dataset = image
                    };

                    imageRequests.Add(request);
                    await client.AddRequestAsync(request);
                }
            }

            await client.AddRequestAsync(
                new DicomNActionRequest(
                    filmSession.SOPClassUID,
                    filmSession.SOPInstanceUID,
                    0x0001));

            await client.SendAsync();
        }
    }
}