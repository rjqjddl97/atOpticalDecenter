using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using MySqlConnector;

namespace PhotoDBLibrary
{
    public class OrderInformation
    {
        public string Department_CD { get; set; }
        public string OrderNumber { get; set; }
        public string ItemNumber { get; set; }
        public string ItemName { get; set; }
        public string Spec { get; set; }
        public string SerialNumber { get; set; }
        public string startDate { get; set; }
        public string EndDate { get; set; }
        public string lotNumber { get; set; }
        public string QRCode { get; set; }
        public string Rohs { get; set; }
        public int StatusType { get; set; }
        public bool ActiveEnable { get; set; }
        public int WorkOrderQTY { get; set; }
        public int RemainQTY { get; set; }
        public int ApplyQTY { get; set; }
    }
    public class DBControl
    {
        public const int QR_CODE_SIZE = 21;
        public string DB_UserID { get; set; } = "autoeqp";
        public string DB_UserPass { get; set; } = "autoeqp1";
        public string DB_IpAddress { get; set; } = "210.124.101.97";
        public string DB_Port { get; set; } = "3306";
        public string DB_Name { get; set; } = "scada";
        public string Order_Info_Table { get; set; } = "pop_work_ord";

        public string[] ColumnName_OrderInfo = new string[16] {"department_cd","work_ord_no","item_no","item_nm","spec","serial_no","work_ord_qty","remain_qty",
            "apply_qty","start_dttm","end_dttm","lot_no","qr_code","rohs","status_type","active_yn"};

        public List<OrderInformation> _rOrderDataList = new List<OrderInformation>();

        public DateTime SearchStartDate = new DateTime();
        public DateTime SearchEndDate = new DateTime();
        public DBControl()
        {

        }
        public DBControl(string ip, string port, string uid, string pass, string db)
        {
            DB_IpAddress = ip;
            DB_Port = port;
            DB_UserID = uid;
            DB_UserPass = pass;
            DB_Name = db;
            _rOrderDataList.Clear();
        }

        public List<OrderInformation> SearchDBOrderInfomation(string keys, string keyvalue)
        {
            if ((DB_IpAddress == "") || (DB_Port == "") || (DB_UserID == "") || (DB_UserPass == "")) return null;
            string strDBCon = string.Format("Server={0};Port={1};Database={2};Uid={3};Pwd={4};", DB_IpAddress, DB_Port, DB_Name, DB_UserID, DB_UserPass);
            using (MySqlConnection connection = new MySqlConnection(strDBCon))
            {
                try
                {
                    List<OrderInformation> SearchList = new List<OrderInformation>();
                    /*       DB 기간범위 내의 데이터 읽기 구문 -        SQL 구문 : SELECT *  FROM 테이블명 WHERE Date_format(날짜컬럼,'%Y-%m-%d') BETWEEN 범위시작일자 AND 범위끝일자;         ,문자열은 ' ' 내에 입력필요!   */
                    string strQuery = string.Format("SELECT * FROM {0};", Order_Info_Table);    // DB 테이블 컬럼명 읽기                                
                    connection.Open();
                    MySqlCommand cmd = new MySqlCommand(strQuery, connection);
                    MySqlDataReader rdr = cmd.ExecuteReader();
                    OrderInformation readData = new OrderInformation();
                    SearchList.Clear();
                    while (rdr.Read())
                    {
                        if (keyvalue == rdr[keys].ToString())
                        {
                            readData.Department_CD = rdr[ColumnName_OrderInfo[0]].ToString();
                            readData.OrderNumber = rdr[ColumnName_OrderInfo[1]].ToString();
                            readData.ItemNumber = rdr[ColumnName_OrderInfo[2]].ToString();
                            readData.ItemName = rdr[ColumnName_OrderInfo[3]].ToString();
                            readData.Spec = rdr[ColumnName_OrderInfo[4]].ToString();
                            readData.SerialNumber = rdr[ColumnName_OrderInfo[5]].ToString();
                            readData.WorkOrderQTY = (int)(Convert.ToDecimal(rdr[ColumnName_OrderInfo[6]].ToString()));
                            readData.RemainQTY = (int)(Convert.ToDecimal(rdr[ColumnName_OrderInfo[7]].ToString()));
                            readData.ApplyQTY = (int)(Convert.ToDecimal(rdr[ColumnName_OrderInfo[8]].ToString()));
                            readData.startDate = rdr[ColumnName_OrderInfo[9]].ToString();
                            readData.EndDate = rdr[ColumnName_OrderInfo[10]].ToString();
                            readData.lotNumber = rdr[ColumnName_OrderInfo[11]].ToString();
                            readData.QRCode = rdr[ColumnName_OrderInfo[12]].ToString();
                            readData.Rohs = rdr[ColumnName_OrderInfo[13]].ToString();
                            readData.StatusType = Convert.ToInt32(rdr[ColumnName_OrderInfo[14]].ToString());
                            //readData.ActiveEnable = Convert.ToBoolean(rdr[ColumnName_OrderInfo[15]].ToString());
                            SearchList.Add(readData);
                        }
                    }
                    rdr.Close();
                    return SearchList;
                }
                catch (Exception ex)
                {
                    return null;
                }
            }
        }
        public bool GetDBColumnName()
        {
            if ((DB_IpAddress == "") || (DB_Port == "") || (DB_UserID == "") || (DB_UserPass == "")) return false;
            string strDBCon = string.Format("Server={0};Port={1};Database={2};Uid={3};Pwd={4};", DB_IpAddress, DB_Port, DB_Name, DB_UserID, DB_UserPass);
            using (MySqlConnection connection = new MySqlConnection(strDBCon))
            {
                try
                {
                    string strQuery = string.Format("SELECT column_name from information_schema.columns WHERE table_schema = '{0}' AND table_name = '{1}';", DB_Name, Order_Info_Table);    // DB 테이블 컬럼명 읽기    
                    connection.Open();
                    MySqlCommand cmd = new MySqlCommand(strQuery, connection);
                    MySqlDataReader rdr = cmd.ExecuteReader();
                    int col_Index = 0;
                    while (rdr.Read())
                    {
                        if (col_Index < ColumnName_OrderInfo.Length)
                            ColumnName_OrderInfo[col_Index++] = rdr[0].ToString();
                    }
                    rdr.Close();

                    return true;
                }
                catch (Exception ex)
                {
                    return false;
                }
            }
        }
        public bool SearchDBforDate(DateTime strat, string keys, string keyvalue)
        {
            bool bret = true;
            if ((DB_IpAddress == "") || (DB_Port == "") || (DB_UserID == "") || (DB_UserPass == "")) return false;

            DateTime endtime = new DateTime();
            endtime = System.DateTime.Now;
            string strDBCon = string.Format("Server={0};Port={1};Database={2};Uid={3};Pwd={4};", DB_IpAddress, DB_Port, DB_Name, DB_UserID, DB_UserPass);
            using (MySqlConnection connection = new MySqlConnection(strDBCon))
            {
                try
                {
                    /*       DB 기간범위 내의 데이터 읽기 구문 -        SQL 구문 : SELECT *  FROM 테이블명 WHERE Date_format(날짜컬럼,'%Y-%m-%d') BETWEEN 범위시작일자 AND 범위끝일자;         ,문자열은 ' ' 내에 입력필요!   */
                    //string strQuery = string.Format("SELECT * FROM {0} WHERE Date_format({1},'%Y-%m-%d') BETWEEN '{2}' AND '{3}';", Order_Info_Table, ColumnName_PCBInfo[0], strat.ToString("yyyy-MM-dd"), endtime.ToString("yyyy-MM-dd"));    // DB 테이블 컬럼명 읽기                                
                    string strQuery = string.Format("SELECT * FROM {0};", Order_Info_Table);    // DB 테이블 컬럼명 읽기                                
                    connection.Open();
                    MySqlCommand cmd = new MySqlCommand(strQuery, connection);
                    MySqlDataReader rdr = cmd.ExecuteReader();
                    OrderInformation readData = new OrderInformation();
                    _rOrderDataList.Clear();
                    while (rdr.Read())
                    {
                        if (keyvalue == rdr[keys].ToString())
                        {
                            readData.Department_CD = rdr[ColumnName_OrderInfo[0]].ToString();
                            readData.OrderNumber = rdr[ColumnName_OrderInfo[1]].ToString();
                            readData.ItemNumber = rdr[ColumnName_OrderInfo[2]].ToString();
                            readData.ItemName = rdr[ColumnName_OrderInfo[3]].ToString();
                            readData.Spec = rdr[ColumnName_OrderInfo[4]].ToString();
                            readData.SerialNumber = rdr[ColumnName_OrderInfo[5]].ToString();
                            readData.WorkOrderQTY = (int)(Convert.ToDecimal(rdr[ColumnName_OrderInfo[6]].ToString()));
                            readData.RemainQTY = (int)(Convert.ToDecimal(rdr[ColumnName_OrderInfo[7]].ToString()));
                            readData.ApplyQTY = (int)(Convert.ToDecimal(rdr[ColumnName_OrderInfo[8]].ToString()));
                            readData.startDate = rdr[ColumnName_OrderInfo[9]].ToString();
                            readData.EndDate = rdr[ColumnName_OrderInfo[10]].ToString();
                            readData.lotNumber = rdr[ColumnName_OrderInfo[11]].ToString();
                            readData.QRCode = rdr[ColumnName_OrderInfo[12]].ToString();
                            readData.Rohs = rdr[ColumnName_OrderInfo[13]].ToString();
                            readData.StatusType = Convert.ToInt32(rdr[ColumnName_OrderInfo[14]].ToString());
                            //readData.ActiveEnable = Convert.ToBoolean(rdr[ColumnName_OrderInfo[15]].ToString());
                            _rOrderDataList.Add(readData);
                        }
                    }
                    rdr.Close();

                    return bret;
                }
                catch (Exception)
                {
                    return false;
                }
            }

        }
        public bool SearchDBforKeyword(string keys, string keyvalue, bool keytype)
        {
            bool bret = true;
            if ((DB_IpAddress == "") || (DB_Port == "") || (DB_UserID == "") || (DB_UserPass == "")) return false;

            DateTime endtime = new DateTime();
            endtime = System.DateTime.Now;
            string strDBCon = string.Format("Server={0};Port={1};Database={2};Uid={3};Pwd={4};", DB_IpAddress, DB_Port, DB_Name, DB_UserID, DB_UserPass);
            using (MySqlConnection connection = new MySqlConnection(strDBCon))
            {
                try
                {
                    /*       DB 기간범위 내의 데이터 읽기 구문 -        SQL 구문 : SELECT *  FROM 테이블명 WHERE Date_format(날짜컬럼,'%Y-%m-%d') BETWEEN 범위시작일자 AND 범위끝일자;         ,문자열은 ' ' 내에 입력필요!   */
                    string strQuery;
                    if (keytype == true)
                        strQuery = string.Format("SELECT * FROM {0} Where {1}='{2}';", Order_Info_Table, keys, keyvalue);    // DB 테이블 컬럼명 읽기                                
                    else
                        strQuery = string.Format("SELECT * FROM {0} Where {1}={2};", Order_Info_Table, keys, keyvalue);    // DB 테이블 컬럼명 읽기                                
                    connection.Open();
                    MySqlCommand cmd = new MySqlCommand(strQuery, connection);
                    MySqlDataReader rdr = cmd.ExecuteReader();
                    OrderInformation readData = new OrderInformation();
                    _rOrderDataList.Clear();

                    while (rdr.Read())
                    {
                        if (keyvalue == rdr[keys].ToString())
                        {
                            readData.Department_CD = rdr[ColumnName_OrderInfo[0]].ToString();
                            readData.OrderNumber = rdr[ColumnName_OrderInfo[1]].ToString();
                            readData.ItemNumber = rdr[ColumnName_OrderInfo[2]].ToString();
                            readData.ItemName = rdr[ColumnName_OrderInfo[3]].ToString();
                            readData.Spec = rdr[ColumnName_OrderInfo[4]].ToString();
                            readData.SerialNumber = rdr[ColumnName_OrderInfo[5]].ToString();
                            readData.WorkOrderQTY = (int)(Convert.ToDecimal(rdr[ColumnName_OrderInfo[6]].ToString()));
                            readData.RemainQTY = (int)(Convert.ToDecimal(rdr[ColumnName_OrderInfo[7]].ToString()));
                            readData.ApplyQTY = (int)(Convert.ToDecimal(rdr[ColumnName_OrderInfo[8]].ToString()));
                            readData.startDate = rdr[ColumnName_OrderInfo[9]].ToString();
                            readData.EndDate = rdr[ColumnName_OrderInfo[10]].ToString();
                            readData.lotNumber = rdr[ColumnName_OrderInfo[11]].ToString();
                            readData.QRCode = rdr[ColumnName_OrderInfo[12]].ToString();
                            readData.Rohs = rdr[ColumnName_OrderInfo[13]].ToString();
                            readData.StatusType = Convert.ToInt32(rdr[ColumnName_OrderInfo[14]].ToString());
                            //readData.ActiveEnable = Convert.ToBoolean(rdr[ColumnName_OrderInfo[15]].ToString());
                            _rOrderDataList.Add(readData);
                        }
                    }
                    rdr.Close();

                    if (_rOrderDataList.Count != 0)
                        return bret;
                    else
                        return false;
                }
                catch (Exception)
                {
                    return false;
                }
            }

        }
        /*
        public PCBInformation AnalysisQRcode(string qrcode, PCBInformation pcbdata)
        {
            // QR 코드를 읽은 후 QR 코드의 정보를 바탕으로 세부 정보를 기입한다. qr코드 예시 B1650002555EJ16R00001
            string itemlist = string.Empty;
            string pcbworkorder = string.Empty;
            string pcbworkserialnum = string.Empty;
            PCBInformation retdata = new PCBInformation();
            retdata = pcbdata;
            if (qrcode.Length == QR_CODE_SIZE)
            {
                
                itemlist = qrcode.Substring(0, 11);                                       // 품목번호 추출.
                pcbworkorder = qrcode.Substring(11, 5);                                   // PCB 작업지시 번호.
                pcbworkserialnum = qrcode.Substring(16, 5);                               // PCB 고유번호.
                byte[] bytearrys = System.Text.Encoding.UTF8.GetBytes(pcbworkorder);

                retdata.PCBLotNumber = findpcbworkyear(bytearrys[0]);    // yyyy;
                retdata.PCBLotNumber += findpcbworkMonth(bytearrys[1]);   // mm
                retdata.PCBLotNumber += pcbworkorder.Substring(2, 2);
                retdata.PCBLotNumber += "-" + pcbworkserialnum;

                retdata = getItemListInfo(itemlist,retdata);
            }
            return retdata;
        }
        public string findpcbworkyear(byte year)
        {
            string retyear = "2000";
            int iyear = 2020, idiv = 0;
            DateTime nowyear = DateTime.Now;


            if ((year >= 'A') && (year <= 'Z'))
            {
                idiv = (nowyear.Year - 2020) / 26;
                if (idiv > 0)
                {
                    iyear += (26 * (idiv)) + (year - 'A');
                }
                else
                    iyear += year - 'A';
                retyear = iyear.ToString();
                return retyear;
            }
            else
                return retyear;
        }
        public string findpcbworkMonth(byte month)
        {
            string retmonth = "0";
            int imonth = 0;

            if ((month >= 'A') && (month <= 'L'))
            {
                imonth = month - 'A' + 1;

                retmonth = string.Format("{0:00}", imonth);
            }
            return retmonth;
        }
        public PCBInformation getItemListInfo(string itemlistnum, PCBInformation pcbdata)
        {
            int readinfosize = _itemList.Count;

            PCBInformation retdata = new PCBInformation();

            retdata = pcbdata;
            for (int i = 0; i < readinfosize; i++)
            {
                if (itemlistnum.Equals(_itemList[i].ItemNumber))
                {
                    retdata.ProductModel = _itemList[i].ProductModel;
                    retdata.PCBModelName = _itemList[i].PCBModelName;
                    retdata.ProductType = _itemList[i].ProductType;
                    retdata.OutputType = _itemList[i].OutputType;
                    retdata.PcbTotalNumber = (uint)(_itemList[i].PCB_RowCount * _itemList[i].PCB_ColumnCount);
                    retdata.PassNumber = 0;
                    retdata.FailNumber = retdata.PcbTotalNumber;            
                }
            }
            return retdata;
        }        
        public bool Initial_System()
        {
            _itemList = iteminfoLoad(iteminfopath);
            return true;
        }
        public List<ItemInfomation> iteminfoLoad(string path)
        {
            try
            {
                StreamReader iteminfo = new StreamReader(path);
                List<ItemInfomation> _retlist = new List<ItemInfomation>();
                _retlist.Clear();
                while (!iteminfo.EndOfStream)
                {
                    ItemInfomation infoitem = new ItemInfomation();
                    string linedata = iteminfo.ReadLine();
                    string[] data = linedata.Split(',');
                    infoitem.ItemNumber = data[0];
                    infoitem.PCBModelName = data[1];
                    infoitem.ProductModel = data[2];
                    infoitem.ProductType = data[3];
                    infoitem.OpMode = data[4];
                    infoitem.OutputType = data[5];
                    infoitem.PCB_Width = Convert.ToDouble(data[6]);
                    infoitem.PCB_Hight = Convert.ToDouble(data[7]);
                    infoitem.PCB_RowCount = Convert.ToUInt32(data[8]);
                    infoitem.PCB_ColumnCount = Convert.ToUInt32(data[9]);
                    _retlist.Add(infoitem);
                }
                return _retlist;
            }
            catch (Exception)
            {
                return null;
            }
        }
        */
    }
}
