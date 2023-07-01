using BulkyBook.DataAccess.Repository.IRepository;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace BulkyBook.DataAccess.Repository
{
    public class Repository<T> : IRepository<T> where T : class
    {
        //creating an interaction between database and repository
        //using dependency injection
       
        private readonly ApplicationDbContext _db;

        //Applying DbSet 
        internal DbSet<T> DbSet;
        public Repository(ApplicationDbContext db)
        {
            _db = db;
          //  _db.Products.Include(u => u.Category).Include(u => u.CoverType);
            this.DbSet = db.Set<T>();   
        }
    
        public void Add(T entity)
        {
            DbSet.Add(entity);
        }
        //ncludeprop - "Category,CoverType": this will make use to be able to include Catogeory and CoverType
        //properties in our repository
        public IEnumerable<T> GetAll(Expression<Func<T, bool>>? filter=null, string? includeProperties = null)
        {
            IQueryable<T> query = DbSet;
            if(filter != null)
            {
                query = query.Where(filter);
            }
            
            if (includeProperties != null)
            {
                foreach(var includeprop in includeProperties.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                {
                    query = query.Include(includeprop);
                }
            }
            return query.ToList();
        }

        public T GetFirstOrDefault(Expression<Func<T, bool>> filter, string? includeProperties = null, bool tracked = true)
        {
            IQueryable<T> query;
            if (tracked)
            {
                query = DbSet;
            }
            else
            {
                query = DbSet.AsNoTracking();
            }
            
          
            query = query.Where(filter);
            if (includeProperties != null)
            {
                foreach (var includeprop in includeProperties.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                {
                    query = query.Include(includeprop);
                }
            }
            return query.FirstOrDefault();
        }

        public void Remove(T entity)
        {
            DbSet.Remove(entity);
        }

        public void RemoveRange(IEnumerable<T> entity)
        {
            DbSet.RemoveRange(entity);
        }
    }
}
