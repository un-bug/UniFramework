using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UniFramework.Runtime
{
    public interface IDataRow
    {
        int Id { get; }
        bool ParseDataRow(string row);
    }

    public abstract class DataRowBase : IDataRow
    {
        public abstract int Id
        {
            get;
        }

        public virtual bool ParseDataRow(string row)
        {
            Debug.LogWarning("[DataRowBase] not implemented ParseDataRow.");
            return false;
        }
    }

    public class DataTableBase
    {
        private readonly string m_Name;

        public DataTableBase(string name)
        {
            m_Name = name ?? string.Empty;
        }
    }

    public sealed class DataTable<T> : DataTableBase where T : IDataRow
    {
        public DataTable(string name) : base(name)
        {
        }
    }

    [DisallowMultipleComponent]
    public sealed class DataTableManager : MonoSingleton<DataTableManager>
    {
        private Dictionary<string, DataTableBase> m_DataTables;

        protected override void OnInit()
        {
            base.OnInit();
            m_DataTables = new Dictionary<string, DataTableBase>();
        }

        public DataTable<T> CreateDataTable<T>() where T : class, IDataRow, new()
        {
            var dataTable = new DataTable<T>(typeof(T).Name);
            m_DataTables.Add(typeof(T).Name, dataTable);
            return dataTable;
        }

        public DataTable<T> GetDataTable<T>() where T : IDataRow
        {
            return InternalGetDataTable(typeof(T).Name) as DataTable<T>;
        }

        private DataTableBase InternalGetDataTable(string dataTableName)
        {
            if (m_DataTables.TryGetValue(dataTableName, out DataTableBase dataTable))
            {
                return dataTable;
            }

            return null;
        }
    }
}