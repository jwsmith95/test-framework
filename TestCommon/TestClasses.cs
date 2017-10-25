using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;
using System.Runtime.CompilerServices;

using System.Data;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text.RegularExpressions;

namespace TestCommon
{
    [Serializable]
    public class ProjectClass
    {
        public Int32 ProjectId { get; set; }
        public Int32 ProjectTfsId { get; set; }
        public String ProjectName { get; set; }

        public ProjectClass()
        {
        }

        /// Created from Data
        public ProjectClass(DataRow Data)
        {
            ProjectId = (Int32)Data["ProjectId"];
            ProjectTfsId = (Int32)Data["ProjectTfsId"];
            ProjectName = (String)Data["ProjectName"];
        }
    }

    [Serializable]
    public class AreaClass
    {
        public Int32 AreaId { get; set; }
        public Int32 ParentAreaId { get; set; }
        public Int32 ProjectId { get; set; }
        public String AreaName { get; set; }

        public AreaClass()
        {
        }

        /// Created from Data
        public AreaClass(DataRow Data)
        {
            AreaId = (Int32)Data["AreaId"];
            ParentAreaId = (Int32)Data["ParentAreaId"];
            ProjectId = (Int32)Data["ProjectId"];
            AreaName = (String)Data["AreaName"];
        }
    }

    [Serializable]
    public class TestClass : ICloneable, ISerializable
    {
        public String ProjectAreaPath { get; set; }
        public String Name { get; set; }
        public Type ClassTypeInfo { get; set; }
        public String TestFunctionName { get; set; }
        public TestParamAttributeList ParamAttribList { get; set; }

        public TestClass()
        {
            ParamAttribList = new TestParamAttributeList();
        }

        private TestClass(
            SerializationInfo s,
            StreamingContext c)
        {
            ProjectAreaPath = s.GetString("Project");
            Name = s.GetString("Name");
            ClassTypeInfo = (Type)s.GetValue("ClassTypeInfo", typeof(Type));
            TestFunctionName = s.GetString("TestFunctionName");
            ParamAttribList = (TestParamAttributeList)s.GetValue("ParamAttribList", typeof(ObservableCollection<TestParamAttribute>));
        }

        public virtual void GetObjectData(
            SerializationInfo s,
            StreamingContext c)
        {
            s.AddValue("Project", ProjectAreaPath);
            s.AddValue("Name", Name);
            s.AddValue("ClassTypeInfo", ClassTypeInfo);
            s.AddValue("TestFunctionName", TestFunctionName);
            s.AddValue("ParamAttribList", ParamAttribList);
        }

        public Object Clone()
        {
            TestClass CloneItem = new TestClass();

            CloneItem.ProjectAreaPath = ProjectAreaPath;
            CloneItem.Name = Name;
            CloneItem.ClassTypeInfo = ClassTypeInfo;
            CloneItem.TestFunctionName = TestFunctionName;

            CloneItem.ParamAttribList = new TestParamAttributeList();

            foreach (TestParamAttribute Enum in ParamAttribList)
            {
                CloneItem.ParamAttribList.Add((TestParamAttribute)Enum.Clone());
            }

            return CloneItem;
        }

        public Byte[] Serialize()
        {
            BinaryFormatter BinFormatter = new BinaryFormatter();
            MemoryStream MemStream = new MemoryStream();
            BinFormatter.Serialize(MemStream, this);

            Byte[] DataBuffer = new Byte[MemStream.Length];

            MemStream.Position = 0;

            Int32 DataLength = MemStream.Read(DataBuffer, 0, DataBuffer.Length);

            return DataBuffer;
        }

        public static TestClass Deserialize(Object DataParam)
        {
            if (DataParam == null || DataParam.GetType() == typeof(DBNull))
                return null;

            Byte[] Data = (Byte[])DataParam;

            BinaryFormatter BinFormatter = new BinaryFormatter();
            return (TestClass)BinFormatter.Deserialize(new MemoryStream(Data));
        }

        public void WriteParametersToFile()
        {
            String TestParameterFileName = ClassTypeInfo.FullName + "." + TestFunctionName + ".prm";

            Stream FileStream = File.Create(TestParameterFileName);

            BinaryFormatter BinFormatter = new BinaryFormatter();

            BinFormatter.Serialize(FileStream, ParamAttribList);

            FileStream.Close();
        }

        public void ReadParametersFromFile()
        {
            String TestParameterFileName = ClassTypeInfo.FullName + "." + TestFunctionName + ".prm";

            if (File.Exists(TestParameterFileName) == false)
                return;

            BinaryFormatter BinFormatter = new BinaryFormatter();

            Stream FileStream = File.OpenRead(TestParameterFileName);

            ParamAttribList = (TestParamAttributeList)BinFormatter.Deserialize(FileStream);

            FileStream.Close();
        }
    }


    [Serializable]
    public class TestScenarioClass
    {
        public Int32 ScenarioId { get; set; }
        public Int32 AreaId { get; set; }
        public String ScenarioName { get; set; }
        public String ScenarioDescription { get; set; }

        /// Created from Data
        public TestScenarioClass(DataRow Data)
        {
            ScenarioId = (Int32)Data["ScenarioId"];
            AreaId = (Int32)Data["AreaId"];
            ScenarioName = (String)Data["ScenarioName"];
            ScenarioDescription = (String)Data["ScenarioDescription"];
        }
    }

    public class TestPlanClass
    {
        public Int32 PlanId = 0;
        public Int32 PlanTfsId = 0;
        public Int32 ProjectId = 0;
        public String PlanName = null;

        public TestPlanClass(
            ProjectClass ProjectParam,
            Int32 PlanTfsIdParam,
            String PlanNameParam)
        {
            PlanTfsId = PlanTfsIdParam;
            ProjectId = ProjectParam.ProjectId;
            PlanName = PlanNameParam;
        }

        /// Created from Data
        public TestPlanClass(DataRow Data)
        {
            PlanId = (Int32)Data["PlanId"];
            ProjectId = (Int32)Data["ProjectId"];
            PlanTfsId = (Int32)Data["PlanTfsId"];
            PlanName = (String)Data["PlanName"];
        }
    }

    public class TestRunClass
    {
        public Int32 RunId { get; set; }
        public Int32 RunTfsId { get; set; }
        public String RunName { get; set; }
        public Int32 PlanId { get; set; }

        public TestRunClass()
        {
        }

        public TestRunClass(
            TestPlanClass PlanParam,
            String RunNameParam)
        {
            PlanId = PlanParam.PlanId;
            RunName = RunNameParam;
        }

        /// Created from Data
        public TestRunClass(DataRow Data)
        {
            RunId = (Int32)Data["RunId"];
            RunTfsId = (Int32)Data["RunTfsId"];
            RunName = (String)Data["RunName"];
            PlanId = (Int32)Data["PlanId"];
        }
    }

    public class TestSuiteClass
    {
        public Int32 SuiteId { get; set; }
        public Int32 SuiteTfsId { get; set; }
        public String SuiteName { get; set; }

        public TestSuiteClass()
        {
        }

        public TestSuiteClass(
            Int32 SuiteTfsIdParam,
            String SuiteNameParam)
        {
            SuiteTfsId = SuiteTfsIdParam;
            SuiteName = SuiteNameParam;
        }

        /// Created from Data
        public TestSuiteClass(DataRow Data)
        {
            SuiteId = (Int32)Data["SuiteId"];
            SuiteName = (String)Data["SuiteName"];
            SuiteTfsId = (Int32)Data["SuiteTfsId"];
        }
    }

    [Serializable]
    public class TestApplicationSettingsClass
    {
        AreaClass AreaMember;

        public AreaClass Area
        {
            get
            {
                return AreaMember;
            }
            set
            {
                if (AreaMember == null ||
                   value.AreaId != AreaMember.AreaId)
                {
                    AreaMember = value;
                    TestScenario = null;
                    TestPlan = null;
                }
            }
        }

        public TestScenarioClass TestScenario { get; set; }
        public String TestPlan { get; set; }

        public Byte[] Serialize()
        {
            BinaryFormatter BinFormatter = new BinaryFormatter();
            MemoryStream MemStream = new MemoryStream();
            BinFormatter.Serialize(MemStream, this);

            Byte[] DataBuffer = new Byte[MemStream.Length];

            MemStream.Position = 0;

            Int32 DataLength = MemStream.Read(DataBuffer, 0, DataBuffer.Length);

            return DataBuffer;
        }

        public static TestClass Deserialize(Object DataParam)
        {
            if (DataParam == null || DataParam.GetType() == typeof(DBNull))
                return null;

            Byte[] Data = (Byte[])DataParam;

            BinaryFormatter BinFormatter = new BinaryFormatter();
            return (TestClass)BinFormatter.Deserialize(new MemoryStream(Data));
        }

        public void WriteToFile()
        {
            String TestParameterFileName = "TestApplicationSettings-" + Environment.UserName + ".prm";

            Stream FileStream = File.Create(TestParameterFileName);

            BinaryFormatter BinFormatter = new BinaryFormatter();

            BinFormatter.Serialize(FileStream, this);

            FileStream.Close();
        }

        public static TestApplicationSettingsClass ReadFromFile()
        {
            String TestParameterFileName = "TestApplicationSettings-" + Environment.UserName + ".prm";

            if (File.Exists(TestParameterFileName) == false)
                return new TestApplicationSettingsClass();

            BinaryFormatter BinFormatter = new BinaryFormatter();

            Stream FileStream = File.OpenRead(TestParameterFileName);

            TestApplicationSettingsClass ReturnData = (TestApplicationSettingsClass)BinFormatter.Deserialize(FileStream);

            FileStream.Close();

            return ReturnData;
        }
    }

    [Serializable]
    public class JobDataClass
    {
        public DateTime RunTime = DateTime.Now;
        //public IterationTypeEnum IterationType;
        public TestParamAttributeList ParamAttribList = new TestParamAttributeList();
    }

    public class JobClass : INotifyPropertyChanged
    {
        public Int32 JobId;
        public String JobOwner;
        public Int32 ProjectId;
        public Int32 AreaId;
        public JobDataClass JobData;

        /// Created by UI
        public JobClass(
            ProjectClass ProjectParam,
            AreaClass AreaParam)
        {
            ProjectId = ProjectParam.ProjectId;
            AreaId = AreaParam.AreaId;

            JobData = new JobDataClass();

            JobData.ParamAttribList.AddIfNotExist(new TestParamTFSProjectAttribute());
            JobData.ParamAttribList.AddIfNotExist(new TestParamTFSAreaAttribute());
            JobData.ParamAttribList.AddIfNotExist(new TestParamTFSTestPlanAttribute());
            JobData.ParamAttribList.AddIfNotExist(new TestParamTFSTestRunAttribute());
            JobData.ParamAttribList.AddIfNotExist(new TestParamThreadCountAttribute());
        }

        /// Create from Data
        public JobClass(DataRow Data)
        {
            JobId = (Int32)Data["JobId"];
            JobName = (String)Data["JobName"];
            JobOwner = (String)Data["JobOwner"];
            ProjectId = (Int32)Data["ProjectId"];
            AreaId = (Int32)Data["AreaId"];
            JobData = (JobDataClass)TestDbClass.DeserializeData(Data["JobData"]);
        }

        String JobNameMember = "New Job";

        public event PropertyChangedEventHandler PropertyChanged;

        public void NotifyPropertyChanged([CallerMemberName] String PropertyName = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(PropertyName));
            }
        }

        public String JobName
        {
            get { return JobNameMember; }
            set
            {
                if (value != JobNameMember)
                {
                    JobNameMember = value;
                    NotifyPropertyChanged();
                }
            }
        }
    }

    public class TaskClass : INotifyPropertyChanged
    {
        public Int32 TaskId;
        public Int32 JobId;

        String TaskNameMember = "New Task";

        public Boolean TaskEnabled { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        /// Created by UI
        public TaskClass(JobClass JobParam)
        {
            TaskEnabled = true;
            JobId = JobParam.JobId;
            Test = new TestClass();
            Test.ParamAttribList.AddIfNotExist(new TestParamTFSTestPlanSuiteAttribute());
        }

        /// Created from Data
        public TaskClass(DataRow Data)
        {
            TaskEnabled = true;
            TaskId = (Int32)Data["TaskId"];
            TaskName = (String)Data["TaskName"];
            TaskEnabled = (Boolean)Data["TaskEnabled"];
            JobId = (Int32)Data["JobId"];
            Test = TestClass.Deserialize(Data["TestData"]);
        }

        public void NotifyPropertyChanged([CallerMemberName] String PropertyName = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(PropertyName));
            }
        }

        public String TaskName
        {
            get { return TaskNameMember; }
            set
            {
                if (value != TaskNameMember)
                {
                    TaskNameMember = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public TestClass Test { get; set; }
    }
}
