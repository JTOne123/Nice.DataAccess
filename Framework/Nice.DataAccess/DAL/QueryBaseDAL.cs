﻿using Nice.DataAccess.Model.Page;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Nice.DataAccess.DAL
{
    public abstract class QueryBaseDAL<T> where T : new()
    {
        /// <summary>
        /// 类型属性集合
        /// </summary>
        private PropertyInfo[] properties;
        private const string flagWhere = " WHERE ";
        private const string flagFrom = " FROM ";
        private const string flagAnd = " AND ";
        private readonly DataHelper DataHelper = null;
        public QueryBaseDAL(string connStrKey)
        {
            GetPropertyInfo();
            DataHelper = DataUtil.GetDataHelper(connStrKey);
        }
        protected void GetPropertyInfo()
        {
            Type type = typeof(T);
            properties = type.GetProperties();
        }
        #region 抽象方法 abstract
        protected abstract string GetPageSql(PageInfo page);
        #endregion

        #region 公共
        private IList<T> GetList(DataTable dt)
        {
            IList<T> result = null;
            if (dt != null && dt.Rows.Count > 0)
            {
                T t = default(T);
                object value = null;
                result = new List<T>();
                IList<PropertyInfo> querypropertys = new List<PropertyInfo>();
                FilterProperty(dt, properties,  querypropertys);
                foreach (DataRow dr in dt.Rows)
                {
                    t = new T();
                    foreach (PropertyInfo pi in querypropertys)
                    {
                        value = dr[pi.Name];
                        if (value != DBNull.Value)
                        {
                            pi.SetValue(t, value, null);
                        }
                    }
                    result.Add(t);
                }
            }
            return result;
        }
        private void FilterProperty(DataTable dt, PropertyInfo[] propertys, IList<PropertyInfo> querypropertys)
        {
            foreach (PropertyInfo pi in propertys)
            {
                if (dt.Columns.Contains(pi.Name))
                {
                    querypropertys.Add(pi);
                }
            }
        }
        /// <summary>
        /// 过滤SQL参数
        /// </summary>
        /// <param name="cmdText"></param>
        private void FilterSQLParmeters(ref string cmdText, IList<object> parmsValue, ref IDataParameter[] parms)
        {
            StringBuilder sb = new StringBuilder();
            parms = new IDataParameter[parmsValue.Count];
            char c;
            int parmIndex = 0;
            for (int i = 0; i < cmdText.Length; i++)
            {
                c = cmdText[i];
                if (c.Equals('?'))
                {
                    sb.AppendFormat("{0}{1}", DataHelper.GetParameterPrefix(), parmIndex);
                    parms[parmIndex] = DataHelper.CreateParameter(DataHelper.GetParameterPrefix() + parmIndex.ToString(), parmsValue[parmIndex]);
                    parmIndex++;
                }
                else
                    sb.Append(c);
            }
            cmdText = sb.ToString();
        }
        #endregion

        #region SQL查询
        /// <summary>
        /// SQL查询对象
        /// </summary>
        /// <param name="IdValue">主键</param>
        /// <returns></returns>
        public T GetBySQL(string cmdText)
        {
            return GetBySQL(cmdText, null);
        }
        /// <summary>
        /// SQL查询对象
        /// </summary>
        /// <param name="IdValue">主键</param>
        /// <returns></returns>

        public T GetBySQL(string cmdText, IList<IDataParameter> parms)
        {
            T t = default(T);
            DataTable dt = DataHelper.ExecuteDataTable(cmdText, CommandType.Text, parms);
            if (dt != null && dt.Rows.Count > 0)
            {
                t = new T();
                object value = null;
                DataRow dr = dt.Rows[0];
                foreach (PropertyInfo pi in properties)
                {
                    if (dt.Columns.Contains(pi.Name))
                    {
                        value = dr[pi.Name];
                        if (value != DBNull.Value)
                        {
                            pi.SetValue(t, value, null);
                        }
                    }
                }
            }
            return t;
        }
        /// <summary>
        /// SQL查询对象
        /// </summary>
        /// <param name="IdValue">主键</param>
        /// <returns></returns>
        public T GetBySQL2(string cmdText, IList<object> parmsValue)
        {
            IDataParameter[] parms = null;
            if (parmsValue != null)
                FilterSQLParmeters(ref cmdText, parmsValue, ref parms);
            return GetBySQL(cmdText, parms);
        }
        /// <summary>
        /// SQL查询列表
        /// </summary>
        /// <param name="IdValue">主键</param>
        /// <returns></returns>
        public IList<T> GetListBySQL(string cmdText)
        {
            return GetList(DataHelper.ExecuteDataTable(cmdText, CommandType.Text, null));
        }
        /// <summary>
        /// SQL查询列表
        /// </summary>
        /// <param name="cmdText">SQL</param>
        /// <returns></returns>
        public IList<T> GetListBySQL(string cmdText, PageInfo page)
        {
            return GetListBySQL(cmdText, null, page);
        }
       

        public IList<T> GetListBySQL(string cmdText, IList<IDataParameter> parms, PageInfo page)
        {
            IList<T> result = null;
            StringBuilder sb = new StringBuilder(100);
            sb.AppendFormat(cmdText);
            if (page != null)
            {
                sb.Append(GetPageSql(page));
                sb.AppendFormat(" SELECT COUNT(1) FROM ({0}) T;", cmdText);
            }
            DataSet ds = DataHelper.ExecuteDataSet(sb.ToString(), CommandType.Text, parms);
            if (ds != null && ds.Tables.Count == 2)
            {
                DataTable dt = ds.Tables[0];
                result = GetList(dt);
                page.TotalCount = int.Parse(ds.Tables[1].Rows[0][0].ToString());
            }
            return result;
        }

        public IList<T> GetListBySQL(string cmdText, IList<IDataParameter> parms)
        {
            DataTable dt = DataHelper.ExecuteDataTable(cmdText, CommandType.Text, parms);
            return GetList(dt);
        }

        /// <summary>
        /// SQL查询列表
        /// </summary>
        /// <param name="cmdText">SQL</param>
        /// <returns></returns>
        public IList<T> GetListBySQL2(string cmdText, IList<object> parmsValue, PageInfo page)
        {
            IDataParameter[] parms = null;
            if (parmsValue != null)
                FilterSQLParmeters(ref cmdText, parmsValue, ref parms);
            return GetListBySQL(cmdText, parms, page);
        }
        /// <summary>
        /// SQL查询列表
        /// </summary>
        /// <param name="IdValue">主键</param>
        /// <returns></returns>
        public IList<T> GetListBySQL2(string cmdText, IList<object> parmsValue)
        {
            IDataParameter[] parms = null;
            if (parmsValue != null)
                FilterSQLParmeters(ref cmdText, parmsValue, ref parms);

            return GetListBySQL(cmdText, parms);
        }
        #endregion

        #region QL
        #region 公共
        /// <summary>
        /// 过滤QL
        /// </summary>
        /// <param name="cmdText"></param>
        private void FilterQueryText(ref string cmdText)
        {
            StringBuilder sb = new StringBuilder();
            cmdText = cmdText.ToUpper();
            int index = cmdText.IndexOf(flagWhere);
            string strSelect = cmdText.Remove(index);
            int indexTable = cmdText.IndexOf(flagFrom);
            string strTable = strSelect.Substring(indexTable + flagFrom.Length);
            sb.Append(strSelect.Remove(indexTable));
            sb.Append(flagFrom);
            //类名转表名
            string[] entityNames = strTable.Split(',');
            string entityName = null;
            for (int i = 0; i < entityNames.Length; i++)
            {
                entityName = entityNames[i];
                sb.Append(DataEntityFactory.EntityAndTables[entityName]);
                if (i != entityNames.Length - 1)
                    sb.Append(',');
            }

            string strWhere = cmdText.Substring(index + flagWhere.Length);
            sb.Append(flagWhere);
            PropertyToColumnName(strWhere, sb);
            cmdText = sb.ToString();
        }
        /// <summary>
        /// 过滤QL
        /// </summary>
        /// <param name="cmdText"></param>
        private void FilterQueryText(ref string cmdText, IList<object> parmsValue, ref IDataParameter[] parms)
        {
            StringBuilder sb = new StringBuilder();
            parms = new IDataParameter[parmsValue.Count];
            cmdText = cmdText.ToUpper();
            int index = cmdText.IndexOf(flagWhere);
            string strSelect = cmdText.Remove(index);
            int indexTable = cmdText.IndexOf(flagFrom);
            string strTable = strSelect.Substring(indexTable + flagFrom.Length);
            sb.Append(strSelect.Remove(indexTable));
            sb.Append(flagFrom);
            //类名转表名
            string[] entityNames = strTable.Split(',');
            string entityName = null;
            for (int i = 0; i < entityNames.Length; i++)
            {
                entityName = entityNames[i];
                sb.Append(DataEntityFactory.EntityAndTables[entityName]);
                if (i != entityNames.Length - 1)
                    sb.Append(',');
            }

            //将属性名转换为表字段名
            string strWhere = cmdText.Substring(index + flagWhere.Length);
            sb.Append(flagWhere);
            int parmIndex = 0;
            PropertyToColumnName(strWhere, sb, parmsValue, parms, ref parmIndex);
            cmdText = sb.ToString();
        }

        /// <summary>
        /// 将属性名转换为表字段名
        /// </summary>
        private void PropertyToColumnName(string strWhere, StringBuilder sb)
        {

            int indexFlag = strWhere.IndexOf('=');
            string nameAndColumn = strWhere.Substring(0, indexFlag).Trim();
            sb.Append(' ');
            sb.Append(DataEntityFactory.PropertyAndColumns[nameAndColumn]);

            string nextNameAndColumn = strWhere.Substring(indexFlag).Trim();
            indexFlag = nextNameAndColumn.IndexOf(flagAnd);

            sb.Append('=');
            if (indexFlag == -1)
            {
                sb.Append(nextNameAndColumn.Trim().TrimStart('='));
                return;
            }
            else
            {
                sb.Append(nextNameAndColumn.Substring(0, indexFlag).Trim().TrimStart('='));
                sb.Append(flagAnd);
                PropertyToColumnName(nextNameAndColumn, sb);
            }
        }
        /// <summary>
        /// 将属性名转换为表字段名
        /// </summary>
        private void PropertyToColumnName(string strWhere, StringBuilder sb, IList<object> parmsValue, IDataParameter[] parms, ref int parmIndex)
        {
            int indexFlag = strWhere.IndexOf('=');
            string columnName = strWhere.Substring(0, indexFlag).Trim();
            sb.Append(' ');
            sb.Append(DataEntityFactory.PropertyAndColumns[columnName]);

            string nextColumnName = strWhere.Substring(indexFlag).Trim();
            indexFlag = nextColumnName.IndexOf(flagAnd);
            string columnValue = null;
            sb.Append('=');
            if (indexFlag == -1)
            {
                columnValue = nextColumnName.Trim().TrimStart('=');
                if (columnValue.Equals("?"))
                {
                    sb.AppendFormat("{0}{1}", DataHelper.GetParameterPrefix(), parmIndex);
                    parms[parmIndex] = DataHelper.CreateParameter(DataHelper.GetParameterPrefix() + parmIndex.ToString(), parmsValue[parmIndex]);
                    parmIndex++;
                }
                else
                {
                    sb.Append(columnValue);
                }
                return;
            }
            else
            {
                columnValue = nextColumnName.Substring(0, indexFlag).Trim().TrimStart('=');
                nextColumnName = nextColumnName.Substring(indexFlag + flagAnd.Length);
                if (columnValue.Equals("?"))
                {
                    sb.AppendFormat("{0}{1}", DataHelper.GetParameterPrefix(), parmIndex);
                    parms[parmIndex] = DataHelper.CreateParameter(DataHelper.GetParameterPrefix() + parmIndex.ToString(), parmsValue[parmIndex]);
                    parmIndex++;
                }
                else
                {
                    sb.Append(columnValue);
                }
                sb.Append(flagAnd);
                PropertyToColumnName(nextColumnName, sb, parmsValue, parms, ref parmIndex);
            }
        }
        #endregion
        /// <summary>
        /// QL查询集合
        /// </summary>
        /// <param name="IdValue">主键</param>
        /// <returns></returns>
        public IList<T> GetList(string cmdText)
        {
            return GetList(cmdText, null);
        }

        /// <summary>
        /// QL查询集合
        /// </summary>
        /// <param name="IdValue">主键</param>
        /// <returns></returns>
        public IList<T> GetList(string cmdText, IList<object> parmsValue)
        {
            IDataParameter[] parms = null;
            if (parmsValue != null)
                FilterQueryText(ref cmdText, parmsValue, ref parms);
            return GetListBySQL2(cmdText, parmsValue);
        }
        #endregion
    }
}
