using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Data.SqlClient;
using System.Collections.ObjectModel;

namespace TestCommon
{
    public class TestDbClass
    {
        public const String AppConnectionString = @"Provider=SQLOLEDB;Data Source=SQL14T;Initial Catalog=QA;Integrated Security=SSPI;";
        public const String WebAppConnectionString = @"Provider=SQLOLEDB;Data Source=SQL14T;Initial Catalog=TestTeamDev;Persist Security Info=True;User ID=TestTeam_Application;Password=Password";

        String ConnectionString;

        public TestDbClass(String ConnectionStringParam = null)
        {
            if (ConnectionStringParam == null)
                ConnectionString = AppConnectionString;
            else
                ConnectionString = ConnectionStringParam;
        }

        public DataTable ExecuteQuery(String QueryString)
        {
            DataTable ReturnData = new DataTable();

            try
            {
                OleDbConnection DbConnection;
                DbConnection = new OleDbConnection(ConnectionString);

                DbConnection.Open();

                OleDbCommand DbCommand = new OleDbCommand(QueryString);

                DbCommand.Connection = DbConnection;

                OleDbDataAdapter DbAdatper = new OleDbDataAdapter();

                DbAdatper.SelectCommand = DbCommand;

                DbAdatper.FillSchema(ReturnData, SchemaType.Source);

                DbAdatper.Fill(ReturnData);

                DbConnection.Close();
            }
            catch (Exception Err)
            {
                throw Err;
            }

            return ReturnData;
        }

        public void ExecuteNonQuery(String CommandString)
        {
            try
            {
                OleDbConnection DbConnection = new OleDbConnection(ConnectionString);

                DbConnection.Open();

                OleDbCommand DbCommand = new OleDbCommand(CommandString);

                DbCommand.Connection = DbConnection;

                DbCommand.ExecuteNonQuery();

                DbConnection.Close();
            }
            catch (Exception Err)
            {
                throw Err;
            }
        }

        public void ExecuteNonQuery(OleDbCommand DbCommand)
        {
            try
            {
                OleDbConnection DbConnection = new OleDbConnection(ConnectionString);

                DbConnection.Open();

                DbCommand.Connection = DbConnection;

                DbCommand.ExecuteNonQuery();

                DbConnection.Close();
            }
            catch (Exception Err)
            {
                throw Err;
            }
        }

        public Int32 ExecuteInsert(OleDbCommand DbCommand)
        {
            try
            {
                Int32 Id = 0;

                OleDbConnection DbConnection = new OleDbConnection(ConnectionString);

                DbConnection.Open();

                DbCommand.Connection = DbConnection;

                DbCommand.ExecuteNonQuery();

                DbCommand.CommandText = "Select @@Identity";

                Id = Convert.ToInt32(DbCommand.ExecuteScalar());

                DbConnection.Close();

                return Id;
            }
            catch (Exception Err)
            {
                throw Err;
            }
        }

        public DataTable ExecuteQueryStoredProcedure(
            String StoreProcedureParam,
            List<OleDbParameter> ParamListParam = null)
        {
            DataTable ReturnData = new DataTable();

            try
            {
                OleDbConnection DbConnection = new OleDbConnection(ConnectionString);

                OleDbCommand DbCommand = new OleDbCommand();
                DbCommand.CommandType = CommandType.StoredProcedure;

                DbCommand.Connection = DbConnection;
                DbCommand.CommandText = StoreProcedureParam;
                DbCommand.CommandType = CommandType.StoredProcedure;

                if (ParamListParam != null)
                {
                    foreach (OleDbParameter ParamEnum in ParamListParam)
                    {
                        DbCommand.Parameters.Add(ParamEnum);
                    }
                }

                DbConnection.Open();

                OleDbDataAdapter DbAdatper = new OleDbDataAdapter();

                DbAdatper.SelectCommand = DbCommand;

                DbAdatper.FillSchema(ReturnData, SchemaType.Source);

                DbAdatper.Fill(ReturnData);

                DbConnection.Close();
            }
            catch (Exception Err)
            {
                throw Err;
            }

            return ReturnData;
        }

        public void ExecuteNonqueryStoredProcedure(
            String StoreProcedureParam,
            List<OleDbParameter> ParamListParam = null)
        {
            try
            {
                OleDbConnection DbConnection = new OleDbConnection(ConnectionString);

                OleDbCommand DbCommand = new OleDbCommand();
                DbCommand.CommandType = CommandType.StoredProcedure;

                DbCommand.Connection = DbConnection;
                DbCommand.CommandText = StoreProcedureParam;
                DbCommand.CommandType = CommandType.StoredProcedure;

                if (ParamListParam != null)
                {
                    foreach (OleDbParameter ParamEnum in ParamListParam)
                    {
                        DbCommand.Parameters.Add(ParamEnum);
                    }
                }

                DbConnection.Open();

                DbCommand.ExecuteNonQuery();

                DbConnection.Close();
            }
            catch (Exception Err)
            {
                throw Err;
            }

            return;
        }

        static public Byte[] SerializeData(Object Data)
        {
            BinaryFormatter BinFormatter = new BinaryFormatter();
            MemoryStream MemStream = new MemoryStream();
            BinFormatter.Serialize(MemStream, Data);

            Byte[] DataBuffer = new Byte[MemStream.Length];

            MemStream.Position = 0;

            MemStream.Read(DataBuffer, 0, DataBuffer.Length);

            return DataBuffer;
        }

        static public Object DeserializeData(Object Data)
        {
            BinaryFormatter BinFormatter = new BinaryFormatter();

            return BinFormatter.Deserialize(new MemoryStream((Byte[])Data));
        }

        public Int32 AddData(
            String ProjectName,
            String AreaPath,
            String ScenarioName,
            String DataName,
            String DataDescription,
            Object Data)
        {
            /// TODO: do not use project anymore
            ProjectClass Project = GetProjects().Where(x => x.ProjectName.Equals(ProjectName)).First();
            if (Project == null)
                throw new Exception("AddData() Project not found");

            AreaClass Area = GetArea(AreaPath);
            if (Area == null)
                throw new Exception("AddData() Area not found");

            TestScenarioClass TestScenario = GetTestScenarios(Area).Where(x => x.ScenarioName.Equals(ScenarioName)).First();
            if (TestScenario == null)
                throw new Exception("AddData() Scneario not found");

            return AddData(
                TestScenario.ScenarioId,
                DataName,
                DataDescription,
                Data);
        }

        public Int32 AddData(
            Int32 ScenarioId,
            String DataName,
            String DataDescription,
            Object Data)
        {
            OleDbCommand DbCommand = new OleDbCommand("INSERT TestData (ScenarioId,DataName,DataDescription,Data) VALUES (?,?,?,?)");

            DbCommand.Parameters.AddWithValue("@ScenarioId", ScenarioId);
            DbCommand.Parameters.AddWithValue("@DataName", DataName);
            DbCommand.Parameters.AddWithValue("@DataDescription", DataDescription);
            DbCommand.Parameters.AddWithValue("@Data", SerializeData(Data));

            return ExecuteInsert(DbCommand);
        }

        public void SaveData(
            Int32 DataId,
            String DataName,
            String DataDescription,
            Object Data)
        {
            /// TODO Query Project throw error

            /// TODO Query Scenario throw error

            // OleDbCommand DbCommand = new OleDbCommand("UPDATE TestData SET DataName='?', DataDescription='?', Data='?' WHERE DataId=?");

            OleDbCommand DbCommand = new OleDbCommand("UPDATE TestData SET DataName=?, DataDescription=?, Data=? WHERE DataId=?");

            DbCommand.Parameters.AddWithValue("@DataName", DataName);
            DbCommand.Parameters.AddWithValue("@DataDescription", DataDescription);
            DbCommand.Parameters.AddWithValue("@Data", SerializeData(Data));
            DbCommand.Parameters.AddWithValue("@DataId", DataId);

            ExecuteNonQuery(DbCommand);

            return;
        }

        public Object GetData(
            String AreaPath,
            String ScenarioName,
            String DataName)
        {
            AreaClass Area = GetArea(AreaPath);
            if (Area == null)
                throw new Exception(String.Format("Could not find area path <{0}>", AreaPath));

            String QueryString = String.Format(
                @"SELECT ScenarioId FROM TestScenarios WHERE ScenarioName='{0}' AND AreaId='{1}'",
                ScenarioName,
                Area.AreaId);

            DataTable QueryData = ExecuteQuery(QueryString);

            Int32 ScenarioId = (Int32)QueryData.Rows[0][0];

            if (QueryData.Rows.Count == 0)
                throw new Exception(String.Format("Could not find Scenario <{0}>", ScenarioName));

            QueryData = ExecuteQuery(
                String.Format("SELECT Data FROM TestData WHERE DataName='" + DataName + "' AND ScenarioId='{0}'",
                ScenarioId));

            if (QueryData.Rows.Count == 0)
                return null;

            return DeserializeData(QueryData.Rows[0]["Data"]);
        }

        public List<Object> GetData(
            String AreaPath,
            String ScenarioName)
        {
            List<Object> returnDataList = new List<Object>();

            AreaClass Area = GetArea(AreaPath);
            if (Area == null)
                throw new Exception(String.Format("Could not find area path <{0}>", AreaPath));

            String QueryString = String.Format(
                @"SELECT ScenarioId FROM TestScenarios WHERE ScenarioName='{0}' AND AreaId='{1}'",
                ScenarioName,
                Area.AreaId);

            DataTable QueryData = ExecuteQuery(QueryString);

            Int32 ScenarioId = (Int32)QueryData.Rows[0][0];

            if (QueryData.Rows.Count == 0)
                throw new Exception(String.Format("Could not find Scenario <{0}>", ScenarioName));

            QueryData = ExecuteQuery(
                String.Format("SELECT Data FROM TestData WHERE ScenarioId='{0}'",
                ScenarioId));

            foreach (DataRow RowEnum in QueryData.Rows)
            {
                returnDataList.Add(DeserializeData(RowEnum["Data"]));
            }

            return returnDataList;
        }

        public Object GetData(
            Int32 ScenarioId,
            String DataName)
        {
            DataTable QueryData = ExecuteQuery("SELECT Data FROM TestData WHERE DataName='" + DataName + "' AND ScenarioId=" + ScenarioId);

            if (QueryData.Rows.Count == 0)
                return null;

            return DeserializeData(QueryData.Rows[0]["Data"]);
        }

        public Object GetData(Int32 DataId)
        {
            BinaryFormatter BinFormatter = new BinaryFormatter();
            DataTable QueryData = ExecuteQuery("SELECT Data FROM TestData WHERE DataId = '" + DataId + "'");

            if (QueryData.Rows.Count != 1)
                throw new Exception("Data not found");

            return DeserializeData(QueryData.Rows[0]["Data"]);
        }

        public void DeleteData(Int32 DataId)
        {
            OleDbCommand DbJobCommand = new OleDbCommand("DELETE FROM TestData WHERE DataId ='" + DataId + "'");
            ExecuteNonQuery(DbJobCommand);
        }

        public List<JobClass> GetJobs()
        {
            List<JobClass> JobList = new List<JobClass>();

            DataTable JobTable = ExecuteQuery("SELECT * FROM TestJobs");

            foreach (DataRow RowEnum in JobTable.Rows)
            {
                JobClass Job = new JobClass(RowEnum);

                JobList.Add(Job);
            }

            return JobList;
        }

        public List<JobClass> GetJobs(
            ProjectClass ProjectParam,
            AreaClass AreaParam)
        {
            List<JobClass> JobList = new List<JobClass>();

            if (ProjectParam == null || AreaParam == null)
                return JobList;

            DataTable JobTable = ExecuteQuery(
                String.Format("SELECT * FROM TestJobs WHERE ProjectId={0} AND AreaId={1}",
                ProjectParam.ProjectId,
                AreaParam.AreaId));

            foreach (DataRow RowEnum in JobTable.Rows)
            {
                JobClass Job = new JobClass(RowEnum);

                JobList.Add(Job);
            }

            return JobList;
        }

        public List<TaskClass> GetTasks(JobClass Job)
        {
            List<TaskClass> TaskList = new List<TaskClass>();

            if (Job == null)
                return TaskList;

            DataTable JobTable = ExecuteQuery("SELECT * FROM TestTasks WHERE JobId = '" + Job.JobId + "' ORDER BY [TaskName]");

            foreach (DataRow RowEnum in JobTable.Rows)
            {
                TaskList.Add(new TaskClass(RowEnum));
            }

            return TaskList;
        }

        public void SaveJob(JobClass Job)
        {
            OleDbCommand DbCommand = null;

            if (Job.JobId == 0)
            {
                DbCommand = new OleDbCommand("INSERT TestJobs (JobName,JobOwner,ProjectId,AreaId,JobData) VALUES (?,?,?,?,?)");
                DbCommand.Parameters.AddWithValue("@JobName", Job.JobName);
                DbCommand.Parameters.AddWithValue("@JobOnwer", Environment.UserName);
                DbCommand.Parameters.AddWithValue("@ProjectId", Job.ProjectId);
                DbCommand.Parameters.AddWithValue("@AreaId", Job.AreaId);
                DbCommand.Parameters.AddWithValue("@JobData", SerializeData(Job.JobData));
                ExecuteInsert(DbCommand);
            }
            else
            {
                DbCommand = new OleDbCommand("UPDATE TestJobs SET JobName=?, JobData=? WHERE JobId=?");
                DbCommand.Parameters.AddWithValue("@JobName", Job.JobName);
                DbCommand.Parameters.AddWithValue("@JobData", SerializeData(Job.JobData));
                DbCommand.Parameters.AddWithValue("@JobId", Job.JobId);
                ExecuteNonQuery(DbCommand);
            }
        }

        public void DeleteJob(JobClass Job)
        {
            OleDbCommand DbJobCommand = new OleDbCommand("DELETE FROM TestJobs WHERE JobId ='" + Job.JobId + "'");
            ExecuteNonQuery(DbJobCommand);

            OleDbCommand DbTaskCommand = new OleDbCommand("DELETE FROM TestTasks WHERE JobId ='" + Job.JobId + "'");
            ExecuteNonQuery(DbTaskCommand);
        }

        public void SaveTask(TaskClass Task)
        {
            OleDbCommand DbCommand = null;

            if (Task.TaskId == 0)
            {
                DbCommand = new OleDbCommand("INSERT TestTasks (TaskName,TaskEnabled,JobId,TestData) VALUES (?,?,?,?)");

                DbCommand.Parameters.AddWithValue("@TaskName", Task.TaskName);
                DbCommand.Parameters.AddWithValue("@TaskEnabled", Task.TaskEnabled);
                DbCommand.Parameters.AddWithValue("@JobId", Task.JobId);

                if (Task.Test != null)
                    DbCommand.Parameters.AddWithValue("@TestData", Task.Test.Serialize());
                else
                    DbCommand.Parameters.AddWithValue("@TestData", null);

                ExecuteInsert(DbCommand);
            }
            else
            {
                DbCommand = new OleDbCommand("UPDATE TestTasks SET TaskName=?, TaskEnabled=?, TestData=? WHERE TaskId=?");

                DbCommand.Parameters.AddWithValue("@TaskName", Task.TaskName);
                DbCommand.Parameters.AddWithValue("@TaskEnabled", Task.TaskEnabled);

                if (Task.Test != null)
                    DbCommand.Parameters.AddWithValue("@TestData", Task.Test.Serialize());
                else
                    DbCommand.Parameters.AddWithValue("@TestData", null);

                DbCommand.Parameters.AddWithValue("@TaskId", Task.TaskId);

                ExecuteNonQuery(DbCommand);
            }
        }

        public void DeleteTask(TaskClass Task)
        {
            OleDbCommand DbCommand = new OleDbCommand("DELETE FROM TestTasks WHERE TaskId ='" + Task.TaskId + "'");
            ExecuteNonQuery(DbCommand);
        }

        public List<ProjectClass> GetProjects()
        {
            List<ProjectClass> ReturnData = new List<ProjectClass>();

            DataTable QueryData = ExecuteQuery(String.Format(@"SELECT * FROM TestProjects ORDER BY ProjectName"));

            foreach (DataRow RowEnum in QueryData.Rows)
            {
                ReturnData.Add(new ProjectClass(RowEnum));
            }

            return ReturnData;
        }

        public List<AreaClass> GetAreas()
        {
            List<AreaClass> ReturnData = new List<AreaClass>();

            DataTable QueryData = ExecuteQuery(String.Format(@"SELECT * FROM TestAreas ORDER BY AreaName"));

            foreach (DataRow RowEnum in QueryData.Rows)
            {
                ReturnData.Add(new AreaClass(RowEnum));
            }

            return ReturnData;
        }

        public List<AreaClass> GetAreas(ProjectClass ProjectParam)
        {
            //return GetAreas().FindAll(x => x.ProjectId.Equals(ProjectParam.ProjectId));

            List<AreaClass> ReturnData = new List<AreaClass>();

            DataTable QueryData = ExecuteQuery(String.Format(@"SELECT * FROM TestAreas WHERE ProjectId={0} ORDER BY AreaName", ProjectParam.ProjectId));

            foreach (DataRow RowEnum in QueryData.Rows)
            {
                ReturnData.Add(new AreaClass(RowEnum));
            }

            return ReturnData;
        }

        public AreaClass GetArea(String AreaPath)
        {
            String[] AreaArray = AreaPath.Split(new Char[] { '\\' });

            AreaClass Area = null;

            for (Int32 AreaIndex = 0; AreaIndex < AreaArray.Count(); AreaIndex++)
            {
                Int32 ParentAreaId = 0;
                String QueryString;

                // First Area, ParentAreaId = 0
                if (Area != null)
                    ParentAreaId = Area.AreaId;

                QueryString = String.Format(@"SELECT * FROM TestAreas WHERE AreaName LIKE '{0}' AND ParentAreaId={1}",
                    AreaArray[AreaIndex],
                    ParentAreaId);

                DataTable QueryData = ExecuteQuery(QueryString);

                if (QueryData.Rows.Count == 0)
                    throw new Exception("Area not found <" + AreaPath + ">");

                Area = new AreaClass(QueryData.Rows[0]);
            }

            return Area;
        }

        public String GetAreaPath(AreaClass AreaParam)
        {
            if (AreaParam == null)
                return "\\";

            String ReturnString = AreaParam.AreaName;

            ProjectClass Project = GetProjects().Where(x => x.ProjectId.Equals(AreaParam.ProjectId)).First();
            List<AreaClass> AreaList = GetAreas(Project).ToList();

            Int32 SrchParentAreaId = AreaParam.ParentAreaId;

            while (SrchParentAreaId != 0)
            {
                AreaClass AreaEnum = AreaList.Where(x => x.AreaId.Equals(SrchParentAreaId)).First();

                ReturnString = AreaEnum.AreaName + "\\" + ReturnString;

                SrchParentAreaId = AreaEnum.ParentAreaId;
            }

            return ReturnString;
        }

        public List<TestScenarioClass> GetTestScenarios(AreaClass AreaParam)
        {
            List<TestScenarioClass> ReturnData = new List<TestScenarioClass>();

            DataTable QueryData = ExecuteQuery(String.Format(@"SELECT * FROM [TestScenarios] WHERE AreaId={0} ORDER BY ScenarioName", AreaParam.AreaId));

            foreach (DataRow RowEnum in QueryData.Rows)
            {
                ReturnData.Add(new TestScenarioClass(RowEnum));
            }

            return ReturnData;
        }        
    }
}
