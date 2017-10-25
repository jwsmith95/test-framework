using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.ComponentModel;
using System.Runtime.Serialization.Formatters.Binary;

namespace TestCommon
{
    [TestParam("Browser", new String[] { "Chrome", "Internet Explorer", "FireFox" })]
    abstract public class WebTestProjectClass
    {
        protected WebTestClass WebTest;

        public void TestStart(ObservableCollection<TestParamAttribute> ParamAttribList)
        {
            String BrowserParam = TestParamManager.GetValueForParameter(
                "Browser",
                ParamAttribList);

            String UrlBase = TestParamManager.GetValueForParameter(
                "URL Base",
                ParamAttribList);

            if (String.Compare(BrowserParam, "Internet Explorer") == 0)
                WebTest = new WebTestClass(WebBrowserTypeEnum.InternetExplorer);
            else if (String.Compare(BrowserParam, "FireFox") == 0)
                WebTest = new WebTestClass(WebBrowserTypeEnum.FireFox);
            else if (String.Compare(BrowserParam, "Chrome") == 0)
                WebTest = new WebTestClass(WebBrowserTypeEnum.Chrome);
            else
            {
                throw new Exception(String.Format("Invalid parameter [Browser:<{0}>]", BrowserParam));
            }

            WebTest.SetUrlBase(UrlBase);

            WebTest.Start();
        }

        public void TestStop()
        {
            WebTest.Stop();
        }
    }

    abstract public class TestProjectClass
    {
        public void TestStart(ObservableCollection<TestParamAttribute> ParamAttribList)
        {
        }

        public void TestStop()
        {
        }
    }

    public class ProjectAttribute : System.Attribute
    {
        public String Name { get; set; }

        public ProjectAttribute(String NameParam)
        {
            Name = NameParam;
        }
    }

    public class TestAttribute : System.Attribute
    {
        public TestAttribute()
        {
        }
    }

    public class TestParamManager
    {
        static public String GetValueForParameter(
            String SrchParameter,
            ObservableCollection<TestParamAttribute> ParamAttribList)
        {
            foreach (TestParamAttribute EnumAttrib in ParamAttribList)
            {
                if (EnumAttrib.Parameter == SrchParameter)
                {
                    return EnumAttrib.Value;
                }
            }

            return null;
        }

        static public List<String> GetValuesForParameter(
            String SrchParameter,
            ObservableCollection<TestParamAttribute> ParamAttribList)
        {
            foreach (TestParamAttribute EnumAttrib in ParamAttribList)
            {
                if (EnumAttrib.Parameter == SrchParameter)
                {
                    return EnumAttrib.ValuesMember;
                }
            }

            return null;
        }
    }

    [Serializable]
    public class TestParamAttributeList : ObservableCollection<TestParamAttribute>
    {
        public void AddIfNotExist(TestParamAttribute TestParamAttributeParam)
        {
            if (this.ToList().Exists(x => x.Equals(TestParamAttributeParam)) == false)
                this.Add(TestParamAttributeParam);
        }

        public void AddIfNotExistFromList(TestParamAttributeList ListParam)
        {
            foreach (TestParamAttribute ParamEnum in ListParam)
            {
                AddIfNotExist(ParamEnum);
            }
        }

        public void RemoveFromList(TestParamAttributeList ListParam)
        {
            foreach (TestParamAttribute ParamEnum in ListParam)
            {
                Remove(ParamEnum);
            }
        }
    }

    [Serializable]
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = true)]
    public class TestParamAttribute : System.Attribute, INotifyPropertyChanged, ICloneable, ISerializable
    {
        public String Parameter { get; set; }
        public String[] Options { get; set; }
        public Boolean IsMultiSelect = false;

        protected TestParamAttribute(
            SerializationInfo s,
            StreamingContext c)
        {
            Parameter = (String)s.GetValue("Parameter", typeof(String));
            Options = (String[])s.GetValue("Options", typeof(String[]));
            IsMultiSelect = (Boolean)s.GetValue("IsMultiSelect", typeof(Boolean));
            ValueMember = (String)s.GetValue("ValueMember", typeof(String));
            ValuesMember = (List<String>)s.GetValue("ValuesMember", typeof(List<String>));
        }

        public virtual void GetObjectData(
            SerializationInfo s,
            StreamingContext c)
        {
            s.AddValue("Parameter", Parameter);
            s.AddValue("Options", Options);
            s.AddValue("IsMultiSelect", IsMultiSelect);
            s.AddValue("ValueMember", ValueMember);
            s.AddValue("ValuesMember", ValuesMember);
        }

        public virtual void OptionsRefresh(ObservableCollection<TestParamAttribute> ParamAttribList)
        {
        }

        public String Value
        {
            get
            {
                if (IsMultiSelect)
                {
                    String ValuesReturn = null;

                    foreach (String ValuesEnum in ValuesMember)
                    {
                        if (ValuesReturn == null)
                            ValuesReturn = ValuesEnum;
                        else
                            ValuesReturn += ("," + ValuesEnum);
                    }

                    if (ValuesReturn == null)
                        ValuesReturn = "";

                    return ValuesReturn;
                }
                else
                    return ValueMember;
            }
            set
            {
                if (value != ValueMember)
                {
                    ValueMember = value;
                    NotifyPropertyChanged();
                }
            }
        }

        String ValueMember;
        public List<String> ValuesMember = new List<String>();

        private TestParamAttribute()
        {
        }

        public TestParamAttribute(String ParameterParam)
        {
            Parameter = ParameterParam;
        }

        public TestParamAttribute(
            String ParameterParam,
            String ValueParam)
        {
            Parameter = ParameterParam;
            Value = ValueParam;
        }

        public TestParamAttribute(
            String ParameterParam,
            String[] OptionsParam)
        {
            Parameter = ParameterParam;
            Options = OptionsParam;
            Value = OptionsParam[0];
        }

        public TestParamAttribute(
            Boolean IsMultiSelectParam,
            String ParameterParam,
            String[] OptionsParam)
        {
            IsMultiSelect = IsMultiSelectParam;
            Parameter = ParameterParam;
            Options = OptionsParam;
            Value = OptionsParam[0];
        }

        public Object Clone()
        {
            TestParamAttribute CloneItem = new TestParamAttribute();

            CloneItem.Parameter = String.Copy(Parameter);

            if (Options != null && Options.Count() != 0)
            {
                CloneItem.Options = new String[Options.Count()];

                for (Int32 i = 0; i < Options.Count(); i++)
                {
                    CloneItem.Options[i] = String.Copy(Options[i]);
                }
            }

            CloneItem.IsMultiSelect = IsMultiSelect;

            if (ValueMember != null)
                CloneItem.ValueMember = String.Copy(ValueMember);

            CloneItem.ValuesMember = new List<String>();

            foreach (String Enum in ValuesMember)
            {
                CloneItem.ValuesMember.Add(String.Copy(Enum));
            }

            return CloneItem;
        }

        public override int GetHashCode()
        {
            return Parameter.GetHashCode();
        }

        public override bool Equals(Object ObjectParam)
        {
            TestParamAttribute TestParam = null;

            try
            {
                TestParam = (TestParamAttribute)ObjectParam;
            }
            catch
            {
                return false;
            }

            if (Parameter == TestParam.Parameter)
                return true;
            else
                return false;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public void NotifyPropertyChanged([CallerMemberName] String PropertyName = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(PropertyName));
            }
        }
    }

    [Serializable]
    public class TestParamTFSProjectAttribute : TestParamAttribute, ICloneable, ISerializable
    {
        public TestParamTFSProjectAttribute(String DefaultProject)
            : base(false, "TFS Project", new String[] { DefaultProject })
        {
        }

        public TestParamTFSProjectAttribute()
            : base(false, "TFS Project", new String[] { "" })
        {
        }

        protected TestParamTFSProjectAttribute(
            SerializationInfo s,
            StreamingContext c)
            : base(s, c)
        {

        }

        public override void GetObjectData(
            SerializationInfo s,
            StreamingContext c)
        {
            base.GetObjectData(s, c);
        }

        public override void OptionsRefresh(ObservableCollection<TestParamAttribute> ParamAttribList)
        {
            //TeamClass Team = new TeamClass("http://tfs2010:8080/tfs");

            //List<String> ProjectList = Team.GetProjects();

            //ProjectList.Sort();

            //if (String.IsNullOrEmpty(Value) == false && ProjectList.Exists(x => x == Value) == false)
            //    ProjectList.Insert(0, Value);

            //base.Options = ProjectList.ToArray();
        }

        new public Object Clone()
        {
            return base.Clone();
        }
    }

    [Serializable]
    public class TestParamTFSAreaAttribute : TestParamAttribute, ICloneable, ISerializable
    {
        public TestParamTFSAreaAttribute()
            : base("TFS Area")
        {
        }

        public TestParamTFSAreaAttribute(
            String AreaPathParam)
            : base("TFS Area", AreaPathParam)
        {
        }

        protected TestParamTFSAreaAttribute(
            SerializationInfo s,
            StreamingContext c)
            : base(s, c)
        {

        }

        public override void GetObjectData(
            SerializationInfo s,
            StreamingContext c)
        {
            base.GetObjectData(s, c);
        }

        public override void OptionsRefresh(ObservableCollection<TestParamAttribute> ParamAttribList)
        {
        }

        new public Object Clone()
        {
            return base.Clone();
        }
    }

    [Serializable]
    public class TestParamTFSTestPlanAttribute : TestParamAttribute, ICloneable
    {
        public TestParamTFSTestPlanAttribute()
            : base(false, "TFS Test Plan", new String[] { "" })
        {
        }

        protected TestParamTFSTestPlanAttribute(
            SerializationInfo s,
            StreamingContext c) : base(s, c)
        {
        }

        public override void GetObjectData(
            SerializationInfo s,
            StreamingContext c)
        {
            base.GetObjectData(s, c);
        }

        public override void OptionsRefresh(ObservableCollection<TestParamAttribute> ParamAttribList)
        {
            //TeamClass Team = new TeamClass("http://tfs2010:8080/tfs");

            //String SrchProject = TestParamManager.GetValueForParameter("TFS Project", ParamAttribList);
            //String SrchAreaPath = TestParamManager.GetValueForParameter("TFS Area", ParamAttribList);

            //List<String> TestPlanList = Team.GetTestPlansByAreaPath(SrchAreaPath);

            //TestPlanList.Sort();

            //if (String.IsNullOrEmpty(Value) == false && TestPlanList.Exists(x => x == Value) == false)
            //    TestPlanList.Insert(0, Value);

            //base.Options = TestPlanList.ToArray();
        }

        new public Object Clone()
        {
            return base.Clone();
        }
    }

    [Serializable]
    public class TestParamTFSTestPlanSuiteAttribute : TestParamAttribute, ICloneable
    {
        public TestParamTFSTestPlanSuiteAttribute()
            : base(false, "TFS Test Plan Suite", new String[] { "" })
        {
        }

        protected TestParamTFSTestPlanSuiteAttribute(
            SerializationInfo s,
            StreamingContext c) : base(s, c)
        {
        }

        public override void GetObjectData(
            SerializationInfo s,
            StreamingContext c)
        {
            base.GetObjectData(s, c);
        }

        public override void OptionsRefresh(ObservableCollection<TestParamAttribute> ParamAttribList)
        {
            //TeamClass Team = new TeamClass("http://tfs2010:8080/tfs");

            //TeamTestPlanClass TestPlan = Team.GetTestPlan(
            //    TestParamManager.GetValueForParameter("TFS Project", ParamAttribList),
            //    TestParamManager.GetValueForParameter("TFS Test Plan", ParamAttribList));

            //if (TestPlan == null)
            //    return;

            //List<String> TestPlanSuiteList = TestPlan.GetSuites();

            //TestPlanSuiteList.Sort();

            //if (String.IsNullOrEmpty(Value) == false && TestPlanSuiteList.Exists(x => x == Value) == false)
            //    TestPlanSuiteList.Insert(0, Value);

            //base.Options = TestPlanSuiteList.ToArray();
        }

        new public Object Clone()
        {
            return base.Clone();
        }
    }

    [Serializable]
    public class TestParamTFSTestRunAttribute : TestParamAttribute, ICloneable
    {
        public TestParamTFSTestRunAttribute()
            : base(false, "TFS Test Run", new String[] { "" })
        {
        }

        protected TestParamTFSTestRunAttribute(
            SerializationInfo s,
            StreamingContext c)
            : base(s, c)
        {
        }

        public override void GetObjectData(
            SerializationInfo s,
            StreamingContext c)
        {
            base.GetObjectData(s, c);
        }

        public override void OptionsRefresh(ObservableCollection<TestParamAttribute> ParamAttribList)
        {
            //TeamClass Team = new TeamClass("http://tfs2010:8080/tfs");

            //TeamTestPlanClass TestPlan = Team.GetTestPlan(
            //    ParamManager.GetValueForParameter("TFS Project", ParamAttribList),
            //    ParamManager.GetValueForParameter("TFS Test Plan", ParamAttribList));

            //if (TestPlan == null)
            //    return;

            //List<String> TestRunList = TestPlan.GetTestRuns().Select(x => x.RunName).ToList();

            //TestRunList.Sort();

            //if (String.IsNullOrEmpty(Value) == false && TestRunList.Exists(x => x == Value) == false)
            //    TestRunList.Insert(0, Value);

            //base.Options = TestRunList.ToArray();
        }

        new public Object Clone()
        {
            return base.Clone();
        }
    }

    [Serializable]
    public class TestParamThreadCountAttribute : TestParamAttribute, ICloneable
    {
        public TestParamThreadCountAttribute()
            : base("Thread Count")
        {
        }

        protected TestParamThreadCountAttribute(
            SerializationInfo s,
            StreamingContext c)
            : base(s, c)
        {
        }

        public override void GetObjectData(
            SerializationInfo s,
            StreamingContext c)
        {
            base.GetObjectData(s, c);
        }

        public override void OptionsRefresh(ObservableCollection<TestParamAttribute> ParamAttribList)
        {
        }

        new public Object Clone()
        {
            return base.Clone();
        }
    }

    public class TestSrchClass
    {
        static public List<TestClass> GetTests(String SrchParam = "")
        {
            if(String.IsNullOrEmpty(SrchParam))
                SrchParam = "";

            List<TestClass> TestListReturn = new List<TestClass>();

            if (ProjectList == null)
                EnumertateTests(out ProjectList, out TestList);

            foreach (TestClass EnumItem in TestList.FindAll(x => x.ProjectAreaPath.StartsWith(SrchParam)))
            {
                TestListReturn.Add((TestClass)EnumItem.Clone());
            }

            return TestListReturn;
        }

        static List<String> ProjectList = null;
        static List<TestClass> TestList = null;

        static void ParseParametersFromMember(
            MemberInfo MemberParam,
            ref TestParamAttributeList ParamAttribList)
        {
            foreach (Attribute MemberCustomAttributeIndex in MemberParam.GetCustomAttributes(true))
            {
                if (MemberCustomAttributeIndex.GetType() == typeof(TestParamAttribute))
                {
                    TestParamAttribute Attrib = (TestParamAttribute)MemberCustomAttributeIndex;
                    ParamAttribList.Add((TestParamAttribute)Attrib.Clone());
                }
            }
        }

        static void ParseParametersFromClass(
            Type TypeParam,
            ref TestParamAttributeList ParamAttribList)
        {
            foreach (Attribute CustomAttributeIndex in TypeParam.GetCustomAttributes(true))
            {
                if (CustomAttributeIndex.GetType() == typeof(TestParamAttribute))
                {
                    TestParamAttribute Attrib = (TestParamAttribute)CustomAttributeIndex;
                    ParamAttribList.Add((TestParamAttribute)Attrib.Clone());
                }
            }
        }

        static void ParseTestsFromClassType(
            String ProjectParam,
            Type ClassTypeParam,
            ref List<TestClass> ProjectTestParam)
        {
            foreach (MemberInfo MemberEnum in ClassTypeParam.GetMembers())
            {
                foreach (Attribute MemberCustomAttributeIndex in MemberEnum.GetCustomAttributes(true))
                {
                    if (MemberCustomAttributeIndex.GetType() == typeof(TestAttribute))
                    {
                        TestClass Test = new TestClass();

                        TestAttribute TestAttrib = (TestAttribute)MemberCustomAttributeIndex;

                        Test.ProjectAreaPath = ProjectParam;
                        Test.Name = MemberEnum.Name;
                        Test.ClassTypeInfo = ClassTypeParam;
                        Test.TestFunctionName = MemberEnum.Name;

                        TestParamAttributeList ParamAttribList = new TestParamAttributeList();

                        ParseParametersFromClass(
                            ClassTypeParam,
                            ref ParamAttribList);

                        ParseParametersFromMember(
                            MemberEnum,
                            ref ParamAttribList);

                        Test.ParamAttribList = ParamAttribList;

                        ProjectTestParam.Add(Test);
                    }
                }
            }
        }

        static void ParseProjectsTestsFromClass(
            Type ClassTypeParam,
            ref List<String> ProjectListParam,
            ref List<TestClass> ProjectTestParam)
        {
            foreach (Attribute CustomAttributeIndex in ClassTypeParam.GetCustomAttributes(true))
            {
                if (CustomAttributeIndex.GetType() == typeof(ProjectAttribute))
                {
                    ProjectAttribute ProjectAttrib = (ProjectAttribute)CustomAttributeIndex;

                    if (ProjectListParam.Contains(ProjectAttrib.Name) == false)
                        ProjectListParam.Add(ProjectAttrib.Name);

                    ParseTestsFromClassType(
                        ProjectAttrib.Name,
                        ClassTypeParam,
                        ref ProjectTestParam);
                }
            }
        }

        static void EnumertateTests(
            out List<String> ProjectListParam,
            out List<TestClass> TestListParam)
        {
            ProjectListParam = new List<String>();
            TestListParam = new List<TestClass>();

            String[] LocalFiles;
            Assembly AssemblyHandle;
            Type[] TypeArrayHandle;

            LocalFiles = Directory.GetFiles(Environment.CurrentDirectory);

            foreach (String FileIndex in LocalFiles)
            {
                FileInfo FileIndexInfo = new FileInfo(FileIndex);

                if (FileIndexInfo.Extension != ".dll")
                    continue;

                try
                {
                    AssemblyHandle = Assembly.LoadFile(FileIndexInfo.FullName);
                    TypeArrayHandle = AssemblyHandle.GetExportedTypes();
                }
                catch (Exception Ex)
                {
                    Ex.ToString();
                    continue;
                }

                foreach (Type ClassIndex in TypeArrayHandle)
                {
                    if (ClassIndex.IsDefined(typeof(ProjectAttribute), true) == false)
                        continue;

                    ParseProjectsTestsFromClass(
                        ClassIndex,
                        ref ProjectListParam,
                        ref TestListParam);
                }
            }
        }
    }

    public class TestDataListAttribute : System.Attribute
    {
    }

    public class TestDataClassAttribute : System.Attribute
    {
        public String ProjectName { get; set; }

        public TestDataClassAttribute(String ProjectNameParam)
        {
            ProjectName = ProjectNameParam;
        }
    }

    [Serializable]
    [TestDataList]
    public class TestDataList<T> : List<T>
    {
        public Type GetElementType()
        {
            return typeof(T);
        }
    }
}
