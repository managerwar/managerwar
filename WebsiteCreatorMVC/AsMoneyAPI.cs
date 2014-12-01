using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Jayrock.JsonRpc;
using Jayrock.Json;
using Jayrock.Json.Conversion;
using System.Security.Cryptography;

namespace asmoneyapi
{
    enum AsmoneyAPIError
    {
        AME_OK,
        AME_InvalidUser,
        AME_InvalidAPIData,
        AME_InvalidIP,
        AME_InvalidIPSetup,
        AME_InvalidCurrency,
        AME_InvalidReceiver,
        AME_NotEnoughMoney,
        AME_APILimitReached,
        AME_Invalid

    }

    class TransAction
    {
        public string action;
        public double value;
        public int batchno;
        public string cur;
        public string payer;
        public DateTime logtime;
        public double fee;
    }

    class AsmoneyAPI
    {
        string m_username,
                m_apiname,
                m_password;
        JsonRpcClient m_client;

        public static string CalculateMD5Hash(string input)
        {
            // step 1, calculate MD5 hash from input
            MD5 md5 = System.Security.Cryptography.MD5.Create();
            byte[] inputBytes = System.Text.Encoding.ASCII.GetBytes(input);
            byte[] hash = md5.ComputeHash(inputBytes);

            // step 2, convert byte array to hex string
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < hash.Length; i++)
            {
                sb.Append(hash[i].ToString("X2"));
            }
            return sb.ToString();
        }


        void ReadTransaction(JsonObject b, ref TransAction o)
        {
            o.value = Convert.ToDouble(b["value"]);
            o.batchno = Convert.ToInt32(b["batchno"]);
            o.cur = b["cur"].ToString();
            //2014-05-23T16:05:00.0000000-0,:00
            string date = b["logtime"].ToString();
            int year = Convert.ToInt32(date.Substring(0, 4));
            int mon = Convert.ToInt32(date.Substring(5, 2));
            int day = Convert.ToInt32(date.Substring(8, 2));
            int h = Convert.ToInt32(date.Substring(11, 2));
            int m = Convert.ToInt32(date.Substring(14, 2));
            int s = Convert.ToInt32(date.Substring(17, 2));
            o.logtime = new DateTime(year, mon, day, h, m, s);
            o.payer = b["payer"].ToString();
            o.action = b["action"].ToString();
            o.fee = Convert.ToDouble(b["fee"]);
        }

        public AsmoneyAPI(string username, string apiname, string apipassword)
        {
            m_username = username;
            m_apiname = apiname;
            m_password = apipassword;
            m_client = new JsonRpcClient();
            m_client.Url = "https://www.asmoney.com/api.ashx";
        }

        public AsmoneyAPIError GetBalance(string currency, out double amount)
        {
            amount = 0;
            string str = m_client.InvokeVargs("getbalance", m_username, m_apiname, m_password, currency).ToString();
            if (str == "Invalid user")
                return AsmoneyAPIError.AME_InvalidUser;
            if (str == "Invalid api data")
                return AsmoneyAPIError.AME_InvalidAPIData;
            if (str == "Invalid IP")
                return AsmoneyAPIError.AME_InvalidIP;
            if (str == "Invalid IP setup")
                return AsmoneyAPIError.AME_InvalidIPSetup;
            if (str == "Invalid")
                return AsmoneyAPIError.AME_InvalidCurrency;
            amount = Convert.ToDouble(str);
            return AsmoneyAPIError.AME_OK;
        }

        public AsmoneyAPIError Transfer(string touser, double amount, string currency, string Memo, out int TransActionID)
        {
            TransActionID = 0;
            string str = m_client.InvokeVargs("transfer", m_username, m_apiname, m_password, amount, currency, touser, Memo).ToString();
            if (str == "Invalid user")
                return AsmoneyAPIError.AME_InvalidUser;
            if (str == "Invalid api data")
                return AsmoneyAPIError.AME_InvalidAPIData;
            if (str == "Invalid IP")
                return AsmoneyAPIError.AME_InvalidIP;
            if (str == "Invalid IP setup")
                return AsmoneyAPIError.AME_InvalidIPSetup;
            if (str == "Invalid")
                return AsmoneyAPIError.AME_InvalidCurrency;
            if (str == "The receiver is not valid")
                return AsmoneyAPIError.AME_InvalidReceiver;
            if (str == "Not enough money")
                return AsmoneyAPIError.AME_NotEnoughMoney;
            if (str == "limit")
                return AsmoneyAPIError.AME_APILimitReached;
            TransActionID = Convert.ToInt32(str);
            return AsmoneyAPIError.AME_OK;
        }

        public AsmoneyAPIError TransferBTC(string BTCaddress, double amount, string currency, string Memo, out int TransActionID)
        {
            TransActionID = 0;
            string str = m_client.InvokeVargs("transferbtc", m_username, m_apiname, m_password, amount, currency, BTCaddress, Memo).ToString();
            if (str == "Invalid user")
                return AsmoneyAPIError.AME_InvalidUser;
            if (str == "Invalid api data")
                return AsmoneyAPIError.AME_InvalidAPIData;
            if (str == "Invalid IP")
                return AsmoneyAPIError.AME_InvalidIP;
            if (str == "Invalid IP setup")
                return AsmoneyAPIError.AME_InvalidIPSetup;
            if (str == "Invalid")
                return AsmoneyAPIError.AME_InvalidCurrency;
            if (str == "The receiver is not valid")
                return AsmoneyAPIError.AME_InvalidReceiver;
            if (str == "Not enough money")
                return AsmoneyAPIError.AME_NotEnoughMoney;
            if (str == "limit")
                return AsmoneyAPIError.AME_APILimitReached;
            TransActionID = Convert.ToInt32(str);
            return AsmoneyAPIError.AME_OK;
        }

        public AsmoneyAPIError TransferLTC(string LTCaddress, double amount, string currency, string Memo, out int TransActionID)
        {
            TransActionID = 0;
            string str = m_client.InvokeVargs("transferltc", m_username, m_apiname, m_password, amount, currency, LTCaddress, Memo).ToString();
            if (str == "Invalid user")
                return AsmoneyAPIError.AME_InvalidUser;
            if (str == "Invalid api data")
                return AsmoneyAPIError.AME_InvalidAPIData;
            if (str == "Invalid IP")
                return AsmoneyAPIError.AME_InvalidIP;
            if (str == "Invalid IP setup")
                return AsmoneyAPIError.AME_InvalidIPSetup;
            if (str == "Invalid")
                return AsmoneyAPIError.AME_InvalidCurrency;
            if (str == "The receiver is not valid")
                return AsmoneyAPIError.AME_InvalidReceiver;
            if (str == "Not enough money")
                return AsmoneyAPIError.AME_NotEnoughMoney;
            if (str == "limit")
                return AsmoneyAPIError.AME_APILimitReached;
            TransActionID = Convert.ToInt32(str);
            return AsmoneyAPIError.AME_OK;
        }

        public AsmoneyAPIError TransferDOGE(string DOGEaddress, double amount, string currency, string Memo, out int TransActionID)
        {
            TransActionID = 0;
            string str = m_client.InvokeVargs("transferdoge", m_username, m_apiname, m_password, amount, currency, DOGEaddress, Memo).ToString();
            if (str == "Invalid user")
                return AsmoneyAPIError.AME_InvalidUser;
            if (str == "Invalid api data")
                return AsmoneyAPIError.AME_InvalidAPIData;
            if (str == "Invalid IP")
                return AsmoneyAPIError.AME_InvalidIP;
            if (str == "Invalid IP setup")
                return AsmoneyAPIError.AME_InvalidIPSetup;
            if (str == "Invalid")
                return AsmoneyAPIError.AME_InvalidCurrency;
            if (str == "The receiver is not valid")
                return AsmoneyAPIError.AME_InvalidReceiver;
            if (str == "Not enough money")
                return AsmoneyAPIError.AME_NotEnoughMoney;
            if (str == "limit")
                return AsmoneyAPIError.AME_APILimitReached;
            TransActionID = Convert.ToInt32(str);
            return AsmoneyAPIError.AME_OK;
        }

        public AsmoneyAPIError TransferPPC(string PPCaddress, double amount, string currency, string Memo, out int TransActionID)
        {
            TransActionID = 0;
            string str = m_client.InvokeVargs("transferppc", m_username, m_apiname, m_password, amount, currency, PPCaddress, Memo).ToString();
            if (str == "Invalid user")
                return AsmoneyAPIError.AME_InvalidUser;
            if (str == "Invalid api data")
                return AsmoneyAPIError.AME_InvalidAPIData;
            if (str == "Invalid IP")
                return AsmoneyAPIError.AME_InvalidIP;
            if (str == "Invalid IP setup")
                return AsmoneyAPIError.AME_InvalidIPSetup;
            if (str == "Invalid")
                return AsmoneyAPIError.AME_InvalidCurrency;
            if (str == "The receiver is not valid")
                return AsmoneyAPIError.AME_InvalidReceiver;
            if (str == "Not enough money")
                return AsmoneyAPIError.AME_NotEnoughMoney;
            if (str == "limit")
                return AsmoneyAPIError.AME_APILimitReached;
            TransActionID = Convert.ToInt32(str);
            return AsmoneyAPIError.AME_OK;
        }

        public AsmoneyAPIError TransferDRK(string DRKaddress, double amount, string currency, string Memo, out int TransActionID)
        {
            TransActionID = 0;
            string str = m_client.InvokeVargs("transferdrk", m_username, m_apiname, m_password, amount, currency, DRKaddress, Memo).ToString();
            if (str == "Invalid user")
                return AsmoneyAPIError.AME_InvalidUser;
            if (str == "Invalid api data")
                return AsmoneyAPIError.AME_InvalidAPIData;
            if (str == "Invalid IP")
                return AsmoneyAPIError.AME_InvalidIP;
            if (str == "Invalid IP setup")
                return AsmoneyAPIError.AME_InvalidIPSetup;
            if (str == "Invalid")
                return AsmoneyAPIError.AME_InvalidCurrency;
            if (str == "The receiver is not valid")
                return AsmoneyAPIError.AME_InvalidReceiver;
            if (str == "Not enough money")
                return AsmoneyAPIError.AME_NotEnoughMoney;
            if (str == "limit")
                return AsmoneyAPIError.AME_APILimitReached;
            TransActionID = Convert.ToInt32(str);
            return AsmoneyAPIError.AME_OK;
        }

        public AsmoneyAPIError GetTransaction(int TransActionID, out TransAction details)
        {
            TransAction o = new TransAction();
            details = o;
            string str = m_client.InvokeVargs("gettransaction", m_username, m_apiname, m_password, TransActionID).ToString();
            if (str == "Invalid user")
                return AsmoneyAPIError.AME_InvalidUser;
            if (str == "Invalid api data")
                return AsmoneyAPIError.AME_InvalidAPIData;
            if (str == "Invalid IP")
                return AsmoneyAPIError.AME_InvalidIP;
            if (str == "Invalid IP setup")
                return AsmoneyAPIError.AME_InvalidIPSetup;
            if (str == "Invalid")
                return AsmoneyAPIError.AME_Invalid;

            JsonObject b = (JsonObject)JsonConvert.Import(str);
            ReadTransaction(b, ref o);
            details = o;

            return AsmoneyAPIError.AME_OK;

        } // GetTransaction

        public AsmoneyAPIError GetHistory(int skip, ref List<TransAction> transactions)
        {
            transactions.Clear();
            string str = m_client.InvokeVargs("history", m_username, m_apiname, m_password, skip).ToString();
            if (str == "Invalid user")
                return AsmoneyAPIError.AME_InvalidUser;
            if (str == "Invalid api data")
                return AsmoneyAPIError.AME_InvalidAPIData;
            if (str == "Invalid IP")
                return AsmoneyAPIError.AME_InvalidIP;
            if (str == "Invalid IP setup")
                return AsmoneyAPIError.AME_InvalidIPSetup;
            if (str == "Invalid")
                return AsmoneyAPIError.AME_OK;

            JsonArray a = (JsonArray)JsonConvert.Import(str);
            foreach (JsonObject b in a)
            {
                TransAction o = new TransAction();
                ReadTransaction(b, ref o);
                transactions.Add(o);
            }

            return AsmoneyAPIError.AME_OK;

        } // GetHistory

        public AsmoneyAPIError GetNewTransActions(ref List<TransAction> transactions)
        {
            transactions.Clear();
            string str = m_client.InvokeVargs("getnewtransactions", m_username, m_apiname, m_password).ToString();
            if (str == "Invalid user")
                return AsmoneyAPIError.AME_InvalidUser;
            if (str == "Invalid api data")
                return AsmoneyAPIError.AME_InvalidAPIData;
            if (str == "Invalid IP")
                return AsmoneyAPIError.AME_InvalidIP;
            if (str == "Invalid IP setup")
                return AsmoneyAPIError.AME_InvalidIPSetup;
            if (str == "Updated")
                return AsmoneyAPIError.AME_OK;
            if (str == "0")
                return AsmoneyAPIError.AME_OK;

            JsonArray a = (JsonArray)JsonConvert.Import(str);
            foreach (JsonObject b in a)
            {
                TransAction o = new TransAction();
                ReadTransaction(b, ref o);
                transactions.Add(o);
            }

            return AsmoneyAPIError.AME_OK;

        } // GetNewTransActions

    }
}
