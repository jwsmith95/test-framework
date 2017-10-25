using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestCommon
{
    public class Verify
    {
        static public void AreEqual(
            Object Expected,
            Object Actual,
            String Message)
        {
            if (Expected.Equals(Actual) == false)
                throw new Exception(Message + " <Expected:" + Expected + "><Actual:" + Actual + ">");
        }

        static public void Strings(
            String Expected,
            String Actual,
            String Message)
        {
            if (String.Compare(Expected, Actual, true) != 0)
                throw new Exception(Message + " <Expected:" + Expected + "><Actual:" + Actual + ">");
        }

        static public void Fail(
            String Message)
        {
            throw new Exception(Message);
        }

        static public void Fail(
            String Message,
            params Object[] Args)
        {
            throw new Exception(String.Format(Message, Args));
        }
    }
}
