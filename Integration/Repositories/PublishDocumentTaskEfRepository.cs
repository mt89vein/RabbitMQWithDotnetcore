using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Domain;
using Integration.Abstractions;
using Integration.Extensions;
using Integration.Models;
using Microsoft.EntityFrameworkCore;

namespace Integration.Repositories
{
    public class PublishDocumentTaskEfRepository : IPublishDocumentTaskRepository
    {
        private readonly PublishDocumentTaskContext _context;

        public PublishDocumentTaskEfRepository(PublishDocumentTaskContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<PublishDocumentTask> GetAsync(Guid id)
        {
            return await _context.FindAsync<PublishDocumentTask>(id);
        }

        public async Task<List<PublishDocumentTaskAttempt>> GetAttemtpsAsync(Guid publishDocumentTaskId)
        {
            return await _context.PublishDocumentTaskAttempts.Where(w => w.PublishDocumentTaskId == publishDocumentTaskId).ToListAsync();
        }

        public async Task<DataPage<PublishDocumentTask>> GetTasksByFilterAsync(PublishDocumentTaskFilter filter = null)
        {
            var query = _context.PublishDocumentTasks.AsQueryable();
            if (filter == null)
            {
                query = query.OrderBy(w => w.Id);

                return new DataPage<PublishDocumentTask>
                {
                    Objects = await query.GetPaged(0, 10).ToListAsync(),
                    Count = await query.CountAsync()
                };
            }

            if (filter.Id.HasValue)
            {
                query = query.Where(w => w.Id == filter.Id);
            }

            if (filter.UserId.HasValue)
            {
                query = query.Where(w => w.UserId == filter.UserId);
            }

            if (filter.OrganizationId.HasValue)
            {
                query = query.Where(w => w.OrganizationId == filter.OrganizationId);
            }

            if (filter.DocumentTypes != null && filter.DocumentTypes.Any())
            {
                query = query.Where(w => filter.DocumentTypes.Contains(w.DocumentType));
            }

            if (filter.DocumentId.HasValue)
            {
                query = query.Where(w => w.DocumentId == filter.DocumentId.Value);
            }

            if (filter.DocumentRevision.HasValue)
            {
                query = query.Where(w => w.DocumentRevision == filter.DocumentRevision.Value);
            }

            if (filter.IsDelivered.HasValue)
            {
                query = query.Where(w => w.IsDelivered == filter.IsDelivered.Value);
            }

            if (filter.States != null && filter.States.Any())
            {
                query = query.Where(w => filter.States.Contains(w.State));
            }

            if (filter.Enqueued != null && filter.Enqueued.Length == 2)
            {
                query = query.Where(w => w.CreatedAt >= filter.Enqueued.FirstOrDefault() &&
                                         w.CreatedAt <= filter.Enqueued.LastOrDefault());
            }

            if (filter.HasEisExceptions.HasValue)
            {
                query = query.Where(w => w.HasEisExceptions == filter.HasEisExceptions);
            }

            query = query.OrderBy(w => w.Id);

            var pageSize = filter.PageSize == 0 ? 10 : filter.PageSize;
            var skip = filter.Page <= 1 ? 0 : (filter.Page - 1) * pageSize;

            return new DataPage<PublishDocumentTask>
            {
                Objects = await query.GetPaged(skip, pageSize).ToListAsync(),
                Count = await query.CountAsync()
            };
        }

        public void Save(PublishDocumentTask publishDocumentTask)
        {
            var task = _context.Find<PublishDocumentTask>(publishDocumentTask.Id);
            if (task == null)
            {
                _context.Add(publishDocumentTask);
            }
            else
            {
                _context.Update(publishDocumentTask);
            }
            
            _context.SaveChanges();
        }
    }
}
