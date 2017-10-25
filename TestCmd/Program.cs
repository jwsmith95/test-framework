using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using TestCommon;

namespace TestCmd
{
    public class TestCmdManager
    {
        public void Help()
        {
            Console.WriteLine("Useage:TestCmd.exe \"title:<TestReportClass Title>\" area:<Area>");
        }

        public void ListAreas()
        {
            List<TestClass> testList = TestSrchClass.GetTests("");

            List<String> areaList = testList.Select(x => x.ProjectAreaPath).Distinct().ToList();

            foreach(String areaEnum in areaList)
            {
                Console.WriteLine(areaEnum);
            }
        }

        public void ExecuteTests(String Title, String AreaSrch)
        {
            List<TestClass> TestList = new List<TestClass>();

            foreach(TestClass TestEnum in TestSrchClass.GetTests(""))
            {
                String ProjectAreaName = TestEnum.ProjectAreaPath + "\\" + TestEnum.Name;

                if(ProjectAreaName.ToUpper().StartsWith(AreaSrch))
                    TestList.Add(TestEnum);
            }

            TestReportListClass TestReportList = new TestReportListClass(Title, TestList);

            foreach(TestReportClass TestReportEnum in TestReportList)
            {
                TestReportEnum.Execute();
            }

            TestReportList.CreateReport();
        }

        public void ProcessArgs(String[] args)
        {
            String title = null;
            String area = null;

            foreach(String argEnum in args)
            {
                if(argEnum.StartsWith("AREA:",StringComparison.OrdinalIgnoreCase))
                {
                    area = argEnum.ToUpper().Substring("AREA:".Length);
                }
                else if(argEnum.StartsWith("TITLE:", StringComparison.OrdinalIgnoreCase))
                {
                    title = argEnum.Substring("TITLE:".Length);
                }
            }

            if(title == null || area == null)
            {
                Help();   
            }
            else
            {
                ExecuteTests(title, area);
            }
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            TestCmdManager cmdManager = new TestCmdManager();

            cmdManager.ProcessArgs(args);
        }
    }

    public class TestReportClass
    {
        public Guid TestId = Guid.NewGuid();
        public Guid ExecutionId = Guid.NewGuid();

        public String TestName { get { return test.Name; } }
        public String Outcome = "";
        public String Message = "";
        public String StackTrace = "";
        public DateTime StartTime;
        public DateTime StopTime;

        TestClass test;

        public TestReportClass(TestClass testParam)
        {
            test = testParam;
        }

        public void Execute()
        {
            Exception finalErr = null;

            Object testClassObject = Activator.CreateInstance(test.ClassTypeInfo);

            Object[] args = new Object[1];
            args[0] = test.ParamAttribList;

            StartTime = DateTime.Now;

            try
            {
                test.ClassTypeInfo.InvokeMember(
                    "TestStart",
                    BindingFlags.InvokeMethod,
                    null,
                    testClassObject,
                    args);

                test.ClassTypeInfo.InvokeMember(
                    test.TestFunctionName,
                    BindingFlags.InvokeMethod,
                    null,
                    testClassObject,
                    args);
            }
            catch(Exception runErr)
            {
                finalErr = runErr;
            }

            // Always Stop
            try
            {
                test.ClassTypeInfo.InvokeMember(
                    "TestStop",
                    BindingFlags.InvokeMethod,
                    null,
                    testClassObject,
                    null);
            }
            catch(Exception stopErr)
            {
                if(finalErr == null)
                    finalErr = stopErr;
            }

            StopTime = DateTime.Now;

            if(finalErr == null)
            {
                Outcome = "Passed";
            }
            else
            {
                Outcome = "Failed";
                Message = finalErr.GetBaseException().Message;
                StackTrace = finalErr.GetBaseException().StackTrace.ToString();
            }
        }
    }

    public class TestReportListClass : List<TestReportClass>
    {
        DateTime StartTime = DateTime.Now;
        String ReportName;
        DateTime StopTime;

        Guid TestListId = Guid.NewGuid();
        String TestReportListName;

        public TestReportListClass(String TestListNameParam, List<TestClass> TestList)
        {
            TestReportListName = TestListNameParam;
            ReportName = "TFSReport" + StartTime.ToString("yyyyMMdd-hhmmss") + ".xml";

            foreach(TestClass TestEnum in TestList)
            {
                Add( new TestReportClass(TestEnum) );
            }
        }

        static String FormatDateTime(DateTime DateTimeParam)
        {
            return DateTimeParam.ToString("yyyy-MM-ddThh:mm:ss.fff0000-07:00");
        }

        static String FormatTimeSpan(
            DateTime StarTimeParam,
            DateTime StopTimeParam)
        {
            TimeSpan Duration = new TimeSpan(StopTimeParam.Ticks - StarTimeParam.Ticks);

            return Duration.ToString(@"hh\:mm\:ss") + ".0000000";
        }

        XmlElement CreateTimesElement(XmlDocument TestReportParam)
        {
            XmlElement ReturnElement = TestReportParam.CreateElement("Times", TestReportParam.DocumentElement.NamespaceURI);

            ReturnElement.SetAttribute("creation", FormatDateTime(StartTime));
            ReturnElement.SetAttribute("start", FormatDateTime(StartTime));
            ReturnElement.SetAttribute("finish", FormatDateTime(StopTime));

            return ReturnElement;
        }

        XmlElement CreateResultsElement(XmlDocument TestReportParam)
        {
            XmlElement ReturnElement = TestReportParam.CreateElement("Results", TestReportParam.DocumentElement.NamespaceURI);

            foreach(TestReportClass TestEnum in this)
            {
                XmlElement UnitTestResultElement = TestReportParam.CreateElement("UnitTestResult", TestReportParam.DocumentElement.NamespaceURI);

                UnitTestResultElement.SetAttribute("executionId", TestEnum.ExecutionId.ToString());
                UnitTestResultElement.SetAttribute("testId", TestEnum.TestId.ToString());
                UnitTestResultElement.SetAttribute("testName", TestEnum.TestName);
                UnitTestResultElement.SetAttribute("computerName", Environment.MachineName);
                UnitTestResultElement.SetAttribute("duration", FormatTimeSpan(TestEnum.StartTime, TestEnum.StopTime));
                UnitTestResultElement.SetAttribute("startTime", FormatDateTime(TestEnum.StartTime));
                UnitTestResultElement.SetAttribute("endTime", FormatDateTime(TestEnum.StopTime));
                UnitTestResultElement.SetAttribute("testType", "13cdc9d9-ddb5-4fa4-a97d-d965ccfc6d4b"); /// Visual Studio Unit Test Type
                UnitTestResultElement.SetAttribute("outcome", TestEnum.Outcome);
                UnitTestResultElement.SetAttribute("testListId", TestListId.ToString());

                XmlElement outputElement = TestReportParam.CreateElement("Output", TestReportParam.DocumentElement.NamespaceURI);
                UnitTestResultElement.AppendChild(outputElement);

                XmlElement errorInfoElement = TestReportParam.CreateElement("ErrorInfo", TestReportParam.DocumentElement.NamespaceURI);
                outputElement.AppendChild(errorInfoElement);

                XmlElement messageElement = TestReportParam.CreateElement("Message", TestReportParam.DocumentElement.NamespaceURI);
                errorInfoElement.AppendChild(messageElement);
                messageElement.InnerText = TestEnum.Message;

                XmlElement stackElement = TestReportParam.CreateElement("StackTrace", TestReportParam.DocumentElement.NamespaceURI);
                errorInfoElement.AppendChild(stackElement);
                stackElement.InnerText = TestEnum.StackTrace;

                ReturnElement.AppendChild(UnitTestResultElement);
            }

            return ReturnElement;
        }

        XmlElement CreateTestDefinitionsElement(XmlDocument TestReportParam)
        {
            XmlElement ReturnElement = TestReportParam.CreateElement("TestDefinitions", TestReportParam.DocumentElement.NamespaceURI);

            foreach(TestReportClass TestEnum in this)
            {
                XmlElement UnitTestElement = TestReportParam.CreateElement("UnitTest", TestReportParam.DocumentElement.NamespaceURI);
                UnitTestElement.SetAttribute("name", TestEnum.TestName);
                UnitTestElement.SetAttribute("id", TestEnum.TestId.ToString());
                ReturnElement.AppendChild(UnitTestElement);

                XmlElement ExecutionElement = TestReportParam.CreateElement("Execution", TestReportParam.DocumentElement.NamespaceURI);
                ExecutionElement.SetAttribute("id", TestEnum.ExecutionId.ToString());
                UnitTestElement.AppendChild(ExecutionElement);

                XmlElement TestMethodElement = TestReportParam.CreateElement("TestMethod", TestReportParam.DocumentElement.NamespaceURI);
                TestMethodElement.SetAttribute("codeBase", ReportName);
                TestMethodElement.SetAttribute("className", TestReportListName);
                TestMethodElement.SetAttribute("name", TestEnum.TestName);
                UnitTestElement.AppendChild(TestMethodElement);
            }

            return ReturnElement;
        }

        XmlElement CreateTestEntriesElement(XmlDocument TestReportParam)
        {
            XmlElement ReturnElement = TestReportParam.CreateElement("TestEntries", TestReportParam.DocumentElement.NamespaceURI);

            foreach(TestReportClass TestEnum in this)
            {
                XmlElement TestEntryElement = TestReportParam.CreateElement("TestEntry", TestReportParam.DocumentElement.NamespaceURI);
                TestEntryElement.SetAttribute("testId", TestEnum.TestId.ToString());
                TestEntryElement.SetAttribute("executionId", TestEnum.ExecutionId.ToString());
                TestEntryElement.SetAttribute("testListId", TestListId.ToString());
                ReturnElement.AppendChild(TestEntryElement);
            }

            return ReturnElement;
        }

        XmlElement CreateTestListElement(XmlDocument TestReportParam)
        {
            XmlElement ReturnElement = TestReportParam.CreateElement("TestLists", TestReportParam.DocumentElement.NamespaceURI);

            XmlElement TestEntryElement = TestReportParam.CreateElement("TestList", TestReportParam.DocumentElement.NamespaceURI);
            TestEntryElement.SetAttribute("name", TestReportListName);
            TestEntryElement.SetAttribute("id", TestListId.ToString());
            ReturnElement.AppendChild(TestEntryElement);

            return ReturnElement;
        }

        XmlElement CreateResultSummaryElement(XmlDocument TestReportParam)
        {
            XmlElement ReturnElement = TestReportParam.CreateElement("ResultSummary", TestReportParam.DocumentElement.NamespaceURI);
            ReturnElement.SetAttribute("outcome", "Completed");

            XmlElement CountersElement = TestReportParam.CreateElement("Counters", TestReportParam.DocumentElement.NamespaceURI);
            CountersElement.SetAttribute("total", this.Count.ToString());
            CountersElement.SetAttribute("executed", this.Count.ToString());
            CountersElement.SetAttribute("passed", this.FindAll(x => x.Outcome.Equals("Passed")).Count.ToString());
            CountersElement.SetAttribute("failed", this.FindAll(x => x.Outcome.Equals("Failed")).Count.ToString());
            ReturnElement.AppendChild(CountersElement);

            XmlElement OutputElement = TestReportParam.CreateElement("Output", TestReportParam.DocumentElement.NamespaceURI);
            ReturnElement.AppendChild(OutputElement);

            return ReturnElement;
        }

        public void CreateReport()
        {
            StopTime = DateTime.Now;

            XmlDocument TestReport = new XmlDocument();

            XmlDeclaration xmlDeclaration = TestReport.CreateXmlDeclaration("1.0", "UTF-8", null);

            TestReport.AppendChild(xmlDeclaration);

            XmlElement TestRunElement = TestReport.CreateElement("TestRun", "http://microsoft.com/schemas/VisualStudio/TeamTest/2010");
            TestRunElement.SetAttribute("id", Guid.NewGuid().ToString());
            TestRunElement.SetAttribute("name", ReportName);
            TestReport.AppendChild(TestRunElement);

            TestRunElement.AppendChild(CreateTimesElement(TestReport));
            TestRunElement.AppendChild(CreateResultsElement(TestReport));
            TestRunElement.AppendChild(CreateTestDefinitionsElement(TestReport));
            TestRunElement.AppendChild(CreateTestEntriesElement(TestReport));
            TestRunElement.AppendChild(CreateTestListElement(TestReport));
            TestRunElement.AppendChild(CreateResultSummaryElement(TestReport));

            TestReport.Save(ReportName);
        }
    }

}
