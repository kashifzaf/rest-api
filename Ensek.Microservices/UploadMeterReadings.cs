using Ensek.Domain.Accounts;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using System.Net;

namespace Ensek.Microservices
{
    /**
     * The UploadMeterReadings class represents an Azure Function responsible for handling the upload of meter readings.
     * It processes the meter readings using an instance of IMeterReadingProcessor.
     */
    public class UploadMeterReadings
    {
        private readonly ILogger _logger;
        private readonly IMeterReadingProcessor _meterReadingProcessor;

        /**
         * Constructs a new instance of the UploadMeterReadings class.
         *
         * @param loggerFactory The logger factory used to create a logger instance.
         * @param meterReadingProcessor The meter reading processor used to process the uploaded meter readings.
         */
        public UploadMeterReadings(ILoggerFactory loggerFactory, IMeterReadingProcessor meterReadingProcessor)
        {
            _logger = loggerFactory.CreateLogger<UploadMeterReadings>();
            _meterReadingProcessor = meterReadingProcessor;
        }

        /**
         * The entry point for the Azure Function.
         * Handles the HTTP POST request for meter reading uploads.
         *
         * @param req The HttpRequestData object representing the incoming HTTP request.
         * @returns The HttpResponseData object representing the response to the HTTP request.
         */
        [Function(nameof(UploadMeterReadings))]
        public async Task<HttpResponseData> Run([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "meter-reading-uploads")] HttpRequestData req)
        {
            try {

                if (req.Body == Stream.Null)
                {
                    _logger.LogInformation("Upload meter reading received null stream");
                    return req.CreateResponse(HttpStatusCode.BadRequest);
                }

                HttpResponseData response = req.CreateResponse(HttpStatusCode.OK);

                _logger.LogInformation("Upload meter reading function is triggered");

                var result = await _meterReadingProcessor.SaveCsvData(req.Body);
                await response.WriteAsJsonAsync(new { Message = result.Message, SuccessfulCount = result.SuccessfulCount, FailedCount = result.FailedCount });

                if (!result.IsSuccessful) {
                    response = req.CreateResponse(HttpStatusCode.BadRequest);
                    _logger.LogInformation("Upload meter reading received bad request");
                } else
                {
                    _logger.LogInformation("Upload meter reading function has been completed and now returning success response");
                }


                return response;
            }
            catch (Exception ex) {
                _logger.LogError(ex, "An error occurred during meter reading upload.");
                return req.CreateResponse(HttpStatusCode.InternalServerError);
            }
        }
    }
}
