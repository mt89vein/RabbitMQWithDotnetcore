using System;
using Domain;
using Microsoft.AspNetCore.Mvc;
using MQ.Interfaces;
using MQ.Messages;

namespace Client.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ValuesController : ControllerBase
    {
        private readonly IDocumentPublishService _documentPublishService;

        public ValuesController(IDocumentPublishService documentPublishService)
        {
            _documentPublishService = documentPublishService ??
                                      throw new ArgumentNullException(nameof(documentPublishService));
        }

        // GET api/values
        [HttpGet]
        public ActionResult Get()
        {
            for (int i = 0; i < 1; i++)
            {
                var message = new PublishQueueMessage
                {
                    DocumentType = DocumentType.One,
                    TimeStamp = DateTime.Now,
                    UserId = i,
                    UserData = new SomeDocumentPublishUserData
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

                _documentPublishService.PublishMessage(message);
            }
            return Ok();
        }

        // GET api/values/5
        [HttpGet("{id}")]
        public ActionResult<string> Get(int id)
        {
            return "value";
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
