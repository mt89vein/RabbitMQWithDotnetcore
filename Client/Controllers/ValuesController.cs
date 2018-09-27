using System;
using Domain;
using Microsoft.AspNetCore.Mvc;
using MQ.Abstractions.Messages;
using MQ.Abstractions.Producers.PublishServices;
using MQ.Messages;

namespace Client.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ValuesController : ControllerBase
    {
        private readonly IDocumentOnePublishProducerService _documentOnePublishProducerService;
        private readonly IDocumentTwoPublishProducerService _documentTwoPublishProducerService;

        public ValuesController(IDocumentOnePublishProducerService documentOnePublishProducerService,
            IDocumentTwoPublishProducerService documentTwoPublishProducerService)
        {
            _documentTwoPublishProducerService = documentTwoPublishProducerService ??
                                                 throw new ArgumentNullException(nameof(documentTwoPublishProducerService));
            _documentOnePublishProducerService = documentOnePublishProducerService ??
                                      throw new ArgumentNullException(nameof(documentOnePublishProducerService));
        }

        // GET api/values
        [HttpGet]
        public ActionResult Get()
        {
            for (int i = 0; i < 5; i++)
            {
                var message = new DocumentPublishEventMessage
                {
                    DocumentType = DocumentType.One,
                    TimeStamp = DateTime.Now,
                    UserId = i,
                    UserInputData = new DocumentOnePublishUserInputData
                    {
                        IsInitialVersion = false,
                        LoadId = String.Empty,
                        Login = "login",
                        Password = "Password",
                        IsPublishToProject = false,
                        Version = 0,
                        RegistryNumber = "123",
                    }
                };

                _documentOnePublishProducerService.PublishMessage(message);
            }
            return Ok();
        }

        // GET api/values/5
        [HttpGet("{id}")]
        public ActionResult<string> Get(int id)
        {
            for (int i = 0; i < 5; i++)
            {
                var message = new DocumentPublishEventMessage
                {
                    DocumentType = DocumentType.Two,
                    TimeStamp = DateTime.Now,
                    UserId = i + 5000,
                    UserInputData = new DocumentTwoPublishUserInputData
                    {
                        LoadId = String.Empty,
                        Login = "login",
                        Password = "Password",
                        Version = 0,
                        RegistryNumber = "123",
                    }
                };

                _documentTwoPublishProducerService.PublishMessage(message);
            }

            return "";
        }

        // POST api/values
        [HttpPost]
        public void Post([FromBody] string value)
        {
        }

        // PUT api/values/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE api/values/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
