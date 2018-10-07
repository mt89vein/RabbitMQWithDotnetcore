using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Domain;
using Microsoft.EntityFrameworkCore;
using MQ.Abstractions.Repositories;
using MQ.Models;

namespace MQ.Repositories
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

            if (filter.HasEisErrors.HasValue)
            {
                query = query.Where(w => w.IsHasEisErrors == filter.HasEisErrors);
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

        public void Insert(PublishDocumentTask publishDocumentTask)
        {
            _context.Add(publishDocumentTask);
            _context.SaveChanges();
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

    public class PagedFilter
    {
        public int Page { get; set; } = 1;

        public int PageSize { get; set; } = 20;
    }

    public class PublishDocumentTaskFilter : PagedFilter
    {
        public Guid? Id { get; set; }

        public DocumentType[] DocumentTypes { get; set; }

        public int? DocumentId { get; set; }

        public int? DocumentRevision { get; set; }

        public DateTime[] Enqueued { get; set; }

        public bool? IsDelivered { get; set; }

        public bool? HasEisErrors { get; set; }

        public int? OrganizationId { get; set; }

        public int? UserId { get; set; }

        public PublishState[] States { get; set; }
    }

    public static class QueryableExtensions
    {
        public static IQueryable<T> GetPaged<T>(this IQueryable<T> data, int? skip, int? take)
        {
            var result = data;
            if (skip.HasValue)
            {
                result = result.Skip(skip.Value);
            }
            if (take.HasValue)
            {
                result = result.Take(take.Value);
            }
            return result;
        }
    }

    /// <summary>
    /// Страница с объектами
    /// </summary>
    /// <typeparam name="TData">Тип данных</typeparam>
    public class DataPage<TData>
    {
        /// <summary>
        /// Общее количество объектов по запросу
        /// </summary>
        public int Count { get; set; }

        /// <summary>
        /// Объекты
        /// </summary>
        public IReadOnlyCollection<TData> Objects { get; set; }
    }
}
