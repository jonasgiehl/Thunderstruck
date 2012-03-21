﻿using System;
using System.Linq;
using Thunderstruck.Internal;

namespace Thunderstruck
{
    public class DataObjectQuery<T> where T : new()
    {
        private string _customProjection;

        public DataObjectQuery() { }

        public DataObjectQuery(string projection) : this()
        {
            _customProjection = String.Format(projection, GetTypeFields(typeof(T)));
        }

        public DataContext DataContext { get; private set; }

        public T First(string where = null, object queryParams = null)
        {
            return Top(1, where, queryParams).FirstOrDefault();
        }

        public T[] Top(int count, string where = null, object queryParams = null)
        {
            var query = String.Format("SELECT TOP {0} {1} {2}", count, GetProjection(), where);

            return Execute(query, queryParams);
        }

        public T[] All(string where = null, object queryParams = null)
        {
            var query = String.Format("SELECT {0} {1}", GetProjection(), where);

            return Execute(query, queryParams);
        }

        public DataObjectQuery<T> With(DataContext dataContext)
        {
            DataContext = dataContext;
            return this;
        }

        public string GetProjection()
        {
            if (_customProjection != null) return _customProjection;

            var targetType = typeof(T);
            var fields = GetTypeFields(targetType);
            var tableName = targetType.Name;

            return String.Format("{0} FROM {1}", fields, tableName);
        }

        private string GetTypeFields(Type targetType)
        {
            return String.Join(", ", DataHelpers.GetValidPropertiesOf(targetType).Select(p => p.Name));
        }

        private T[] Execute(string query, object queryParams = null)
        {
            var dataContext = DataContext ?? new DataContext(Transaction.No);

            try
            {
                return dataContext.All<T>(query, queryParams);
            }
            finally
            {
                if (DataContext == null) dataContext.Dispose();
            }
        }
    }
}