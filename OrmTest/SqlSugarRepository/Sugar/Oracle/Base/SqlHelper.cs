﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Oracle.ManagedDataAccess.Client;
using System.Data;

namespace OracleSugar
{
    /// <summary>
    /// ** 描述：底层SQL辅助函数
    /// ** 创始时间：2015-7-13
    /// ** 修改时间：-
    /// ** 作者：sunkaixuan
    /// ** 使用说明：
    /// </summary>
    public class SqlHelper : IDisposable
    {
        OracleConnection _OracleConnection;
        OracleTransaction _tran = null;
        /// <summary>
        /// 如何解释命令字符串 默认为Text 
        /// </summary>
        public CommandType CommandType = CommandType.Text;
        /// <summary>
        /// 是否启用日志事件(默认为:false)
        /// </summary>
        public bool IsEnableLogEvent = false;
        /// <summary>
        /// 执行访数据库前的回调函数  (sql,pars)=>{}
        /// </summary>
        public Action<string, string> LogEventStarting = null;
        /// <summary>
        /// 执行访数据库后的回调函数  (sql,pars)=>{}
        /// </summary>
        public Action<string, string> LogEventCompleted = null;
        /// <summary>
        /// 是否清空OracleParameters
        /// </summary>
        public bool IsClearParameters = true;
        /// <summary>
        /// 设置在终止执行命令的尝试并生成错误之前的等待时间。（单位：秒）
        /// </summary>
        public int CommandTimeOut = 30000;
        /// <summary>
        /// 将页面参数自动填充到OracleParameter []，无需在程序中指定参数
        /// 例如：
        ///     var list = db.Queryable&lt;Student&gt;().Where("id=@id").ToList();
        ///     以前写法
        ///     var list = db.Queryable&lt;Student&gt;().Where("id=@id", new { id=Request["id"] }).ToList();
        /// </summary>
        public bool IsGetPageParas = false;
        /// <summary>
        /// 初始化 SqlHelper 类的新实例
        /// </summary>
        /// <param name="connectionString"></param>
        public SqlHelper(string connectionString)
        {
            _OracleConnection = new OracleConnection(connectionString);
            _OracleConnection.Open();
        }
        /// <summary>
        /// 获取当前数据库连接对象
        /// </summary>
        /// <returns></returns>

        public OracleConnection GetConnection()
        {
            return _OracleConnection;
        }

        /// <summary>
        /// 开始事务
        /// </summary>
        public void BeginTran()
        {
            _tran = _OracleConnection.BeginTransaction();
        }

        /// <summary>
        /// 开始事务
        /// </summary>
        /// <param name="iso">指定事务行为</param>
        public void BeginTran(IsolationLevel iso)
        {
            _tran = _OracleConnection.BeginTransaction(iso);
        }
    
        /// <summary>
        /// 回滚事务
        /// </summary>
        public void RollbackTran()
        {
            if (_tran != null)
            {
                _tran.Rollback();
                _tran = null;
            }
        }

        /// <summary>
        /// 提交事务
        /// </summary>
        public void CommitTran()
        {
            if (_tran != null)
            {
                _tran.Commit();
                _tran = null;
            }
        }

        /// <summary>
        /// 获取第一行第一列
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="pars">匿名参数(例如:new{id=1,name="张三"})</param>
        /// <returns></returns>
        public string GetString(string sql, object pars)
        {
            return GetString(sql, SqlSugarTool.GetParameters(pars));
        }

        /// <summary>
        /// 获取第一行第一列
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="pars"></param>
        /// <returns></returns>
        public string GetString(string sql, params OracleParameter[] pars)
        {
            return Convert.ToString(GetScalar(sql, pars));
        }

        /// <summary>
        /// 获取第一行第一列
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="pars">匿名参数(例如:new{id=1,name="张三"})</param>
        /// <returns></returns>
        public int GetInt(string sql, object pars)
        {
            return GetInt(sql, SqlSugarTool.GetParameters(pars));
        }

        /// <summary>
        /// 获取第一行第一列
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="pars"></param>
        /// <returns></returns>
        public int GetInt(string sql, params OracleParameter[] pars)
        {
            return Convert.ToInt32(GetScalar(sql, pars));
        }

        /// <summary>
        /// 获取第一行第一列
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="pars"></param>
        /// <returns></returns>
        public Double GetDouble(string sql, params OracleParameter[] pars)
        {
            return Convert.ToDouble(GetScalar(sql, pars));
        }

        /// <summary>
        /// 获取第一行第一列
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="pars"></param>
        /// <returns></returns>
        public decimal GetDecimal(string sql, params OracleParameter[] pars)
        {
            return Convert.ToDecimal(GetScalar(sql, pars));
        }

        /// <summary>
        /// 获取第一行第一列
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="pars"></param>
        /// <returns></returns>
        public DateTime GetDateTime(string sql, params OracleParameter[] pars)
        {
            return Convert.ToDateTime(GetScalar(sql, pars));
        }

        /// <summary>
        /// 获取第一行第一列
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="pars">匿名参数(例如:new{id=1,name="张三"})</param>
        /// <returns></returns>
        public object GetScalar(string sql, object pars)
        {
            return GetScalar(sql, SqlSugarTool.GetParameters(pars));
        }

        /// <summary>
        /// 获取第一行第一列
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="pars"></param>
        /// <returns></returns>
        public object GetScalar(string sql, params OracleParameter[] pars)
        {
            OracleConfig.SetParsName(pars);
            sql =OracleConfig.GetOracleSql(sql);
            ExecLogEvent(sql, pars, true);
            OracleCommand OracleCommand = new OracleCommand(sql, _OracleConnection);
            OracleCommand.BindByName = true;
            OracleCommand.CommandType = CommandType;
            if (_tran != null)
            {
                OracleCommand.Transaction = _tran;
            }
            OracleCommand.CommandTimeout = this.CommandTimeOut;
            if (pars != null)
                OracleCommand.Parameters.AddRange(pars);
            if (IsGetPageParas)
            {
                SqlSugarToolExtensions.RequestParasToOracleParameters(OracleCommand.Parameters);
            }
            object scalar = OracleCommand.ExecuteScalar();
            scalar = (scalar == null ? 0 : scalar);
            if (IsClearParameters)
                OracleCommand.Parameters.Clear();
            ExecLogEvent(sql, pars, false);
            return scalar;
        }

        /// <summary>
        /// 执行SQL返回受影响行数
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="pars">匿名参数(例如:new{id=1,name="张三"})</param>
        /// <returns></returns>
        public int ExecuteCommand(string sql, object pars)
        {
            return ExecuteCommand(sql, SqlSugarTool.GetParameters(pars));
        }

        /// <summary>
        /// 执行SQL返回受影响行数
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="pars"></param>
        /// <returns></returns>
        public int ExecuteCommand(string sql, params OracleParameter[] pars)
        {
            OracleConfig.SetParsName(pars);
            sql = OracleConfig.GetOracleSql(sql);
            ExecLogEvent(sql, pars, true);
            OracleCommand OracleCommand = new OracleCommand(sql, _OracleConnection);
            OracleCommand.BindByName = true;
            OracleCommand.CommandType = CommandType;
            OracleCommand.CommandTimeout = this.CommandTimeOut;
            if (_tran != null)
            {
                OracleCommand.Transaction = _tran;
            }
            if (pars != null)
                OracleCommand.Parameters.AddRange(pars);
            if (IsGetPageParas)
            {
                SqlSugarToolExtensions.RequestParasToOracleParameters(OracleCommand.Parameters);
            }
            int count = OracleCommand.ExecuteNonQuery();
            if (IsClearParameters)
                OracleCommand.Parameters.Clear();
            ExecLogEvent(sql, pars, false);
            return count;
        }

        /// <summary>
        /// 获取DataReader
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="pars">匿名参数(例如:new{id=1,name="张三"})</param>
        /// <returns></returns>
        public OracleDataReader GetReader(string sql, object pars)
        {
            return GetReader(sql, SqlSugarTool.GetParameters(pars));
        }

        /// <summary>
        /// 获取DataReader
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="pars"></param>
        /// <returns></returns>
        public OracleDataReader GetReader(string sql, params OracleParameter[] pars)
        {
            OracleConfig.SetParsName(pars);
            sql = OracleConfig.GetOracleSql(sql);
            ExecLogEvent(sql, pars, true);
            OracleCommand OracleCommand = new OracleCommand(sql, _OracleConnection);
            OracleCommand.BindByName = true;
            OracleCommand.CommandType = CommandType;
            OracleCommand.CommandTimeout = this.CommandTimeOut;
            if (_tran != null)
            {
                OracleCommand.Transaction = _tran;
            }
            if (pars != null)
                OracleCommand.Parameters.AddRange(pars);
            if (IsGetPageParas)
            {
                SqlSugarToolExtensions.RequestParasToOracleParameters(OracleCommand.Parameters);
            }
            OracleDataReader OracleDataReader = OracleCommand.ExecuteReader();
            if (IsClearParameters)
                OracleCommand.Parameters.Clear();
            ExecLogEvent(sql, pars, false);
            return OracleDataReader;
        }

        /// <summary>
        /// 根据SQL获取T的集合
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sql"></param>
        /// <param name="pars">匿名参数(例如:new{id=1,name="张三"})</param>
        /// <returns></returns>
        public List<T> GetList<T>(string sql, object pars)
        {
            return GetList<T>(sql, SqlSugarTool.GetParameters(pars));
        }

        /// <summary>
        /// 根据SQL获取T的集合
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sql"></param>
        /// <param name="pars"></param>
        /// <returns></returns>
        public List<T> GetList<T>(string sql, params OracleParameter[] pars)
        {
            var reval = SqlSugarTool.DataReaderToList<T>(typeof(T), GetReader(sql, pars), null);
            return reval;
        }

        /// <summary>
        /// 根据SQL获取T
        /// </summary>
        /// <typeparam name="T">可以是int、string等，也可以是类或者数组、字典</typeparam>
        /// <param name="sql"></param>
        /// <param name="pars">匿名参数(例如:new{id=1,name="张三"})</param>
        /// <returns></returns>
        public T GetSingle<T>(string sql, object pars)
        {
            return GetSingle<T>(sql, SqlSugarTool.GetParameters(pars));
        }

        /// <summary>
        /// 根据SQL获取T
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sql"></param>
        /// <param name="pars"></param>
        /// <returns></returns>
        public T GetSingle<T>(string sql, params OracleParameter[] pars)
        {
            var reval = SqlSugarTool.DataReaderToList<T>(typeof(T), GetReader(sql, pars), null).Single();
            return reval;
        }

        /// <summary>
        /// 获取DataTable
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="pars">匿名参数(例如:new{id=1,name="张三"})</param>
        /// <returns></returns>
        public DataTable GetDataTable(string sql, object pars)
        {
            return GetDataTable(sql, SqlSugarTool.GetParameters(pars));
        }

        /// <summary>
        /// 获取DataTable
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="pars"></param>
        /// <returns></returns>
        public DataTable GetDataTable(string sql, params OracleParameter[] pars)
        {
            OracleConfig.SetParsName(pars);
            sql = OracleConfig.GetOracleSql(sql);
            ExecLogEvent(sql, pars, true);
            OracleDataAdapter _OracleDataAdapter = new OracleDataAdapter(sql, _OracleConnection);
            _OracleDataAdapter.SelectCommand.CommandType = CommandType;
            _OracleDataAdapter.SelectCommand.BindByName = true;
            if (pars != null)
                _OracleDataAdapter.SelectCommand.Parameters.AddRange(pars);
            if (IsGetPageParas)
            {
                SqlSugarToolExtensions.RequestParasToOracleParameters(_OracleDataAdapter.SelectCommand.Parameters);
            }
            _OracleDataAdapter.SelectCommand.CommandTimeout = this.CommandTimeOut;
            if (_tran != null)
            {
                _OracleDataAdapter.SelectCommand.Transaction = _tran;
            }
            DataTable dt = new DataTable();
            _OracleDataAdapter.Fill(dt);
            if (IsClearParameters)
                _OracleDataAdapter.SelectCommand.Parameters.Clear();
            ExecLogEvent(sql, pars, false);
            return dt;
        }
        /// <summary>
        /// 获取DataSet
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="pars"></param>
        /// <returns></returns>
        public DataSet GetDataSetAll(string sql, object pars)
        {
            return GetDataSetAll(sql, SqlSugarTool.GetParameters(pars));
        }
        /// <summary>
        /// 获取DataSet
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="pars"></param>
        /// <returns></returns>
        public DataSet GetDataSetAll(string sql, params OracleParameter[] pars)
        {
            OracleConfig.SetParsName(pars);
            sql = OracleConfig.GetOracleSql(sql);
            ExecLogEvent(sql, pars, true);
            OracleDataAdapter _OracleDataAdapter = new OracleDataAdapter(sql, _OracleConnection);
            if (_tran != null)
            {
                _OracleDataAdapter.SelectCommand.Transaction = _tran;
            }
            if (IsGetPageParas)
            {
                SqlSugarToolExtensions.RequestParasToOracleParameters(_OracleDataAdapter.SelectCommand.Parameters);
            }
            _OracleDataAdapter.SelectCommand.CommandTimeout = this.CommandTimeOut;
            _OracleDataAdapter.SelectCommand.CommandType = CommandType;
            _OracleDataAdapter.SelectCommand.BindByName = true;
            if (pars != null)
                _OracleDataAdapter.SelectCommand.Parameters.AddRange(pars);
            DataSet ds = new DataSet();
            _OracleDataAdapter.Fill(ds);
            if (IsClearParameters)
                _OracleDataAdapter.SelectCommand.Parameters.Clear();
            ExecLogEvent(sql, pars, false);
            return ds;
        }

        private void ExecLogEvent(string sql, OracleParameter[] pars, bool isStarting = true)
        {
            if (IsEnableLogEvent)
            {
                Action<string, string> action = isStarting ? LogEventStarting : LogEventCompleted;
                if (action != null)
                {
                    if (pars == null || pars.Length == 0)
                    {
                        action(sql, null);
                    }
                    else
                    {
                        action(sql, JsonConverter.Serialize(pars.Select(it => new { key = it.ParameterName, value = it.Value })));
                    }
                }
            }
        }
        /// <summary>
        /// 释放数据库连接对象
        /// </summary>
        public void Dispose()
        {
            if (_OracleConnection != null)
            {
                if (_OracleConnection.State != ConnectionState.Closed)
                {
                    if (_tran != null)
                        _tran.Commit();
                    _OracleConnection.Close();
                }
                _OracleConnection = null;
            }
        }
    }
}
