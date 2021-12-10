using Indusoft.LDS.Script.Common;
using Indusoft.LDS.Services.Contracts.Repository;
using Indusoft.LDS.Services.Contracts.Repository.DataServiceFactory;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Indusoft.LDS.Usefull.Rounding
{
    internal class ServiceFactory : IRepositoryDataServiceFactory
    {
        private IScriptSession Session;
        private string ConnectionString;
        private WorkType ConnWorkType;
        MidpointRounding Rounding;
        string Separator;

        public ServiceFactory(IScriptSession session)
        {
            Session = session;
            ConnWorkType = WorkType.Session;
        }
        public ServiceFactory(string connectionString)
        {
            ConnectionString = connectionString;
            ConnWorkType = WorkType.Direct;
        }
        public ServiceFactory(MidpointRounding rounding, string separator)
        {
            Rounding = rounding;
            Separator = separator;
            ConnWorkType = WorkType.Manual;
        }
        public ServiceFactory()
        {
            ConnWorkType = WorkType.Manual;
        }
        public IRepositoryDataService Create()
        {
            switch (ConnWorkType)
            {
                case WorkType.Direct:
                    return new RepositoryDataService(ConnectionString, ConnWorkType);
                case WorkType.Manual:
                    return new RepositoryDataService(Rounding, Separator, ConnWorkType);
                case WorkType.Session:
                default:
                    return new RepositoryDataService(Session, ConnWorkType);
            }
        }
        public enum WorkType
        {
            Session,
            Direct,
            Manual
        }
    }

    internal class RepositoryDataService : IRepositoryDataService
    {
        private IScriptSession Session;
        private string ConnectionString;
        private ServiceFactory.WorkType ConnWorkType;
        MidpointRounding Rounding;
        string Separator;

        public RepositoryDataService(ServiceFactory.WorkType workType)
        {
            ConnWorkType = workType;
        }
        public RepositoryDataService(IScriptSession session, ServiceFactory.WorkType workType) : this(workType)
        {
            Session = session;
        }
        public RepositoryDataService(string connectionString, ServiceFactory.WorkType workType) : this(workType)
        {
            ConnectionString = connectionString;
        }
        public RepositoryDataService(MidpointRounding rounding, string separator, ServiceFactory.WorkType workType) : this(workType)
        {
            Rounding = rounding;
            Separator = separator;
        }

        private SysDatabaseProp GetThroughSession(string prop)
        {
            DataSet ds = g.ExecuteSql(Session, "SysDatabaseProps");
            DataRow dataRow;
            try
            {
                dataRow = ds.Tables[0].Select(string.Format("PropName = '{0}'", prop)).FirstOrDefault();
            }
            catch
            {
                throw new MissingFieldException();
            }
            return new SysDatabaseProp() 
            { 
                PropDesc = dataRow["PropDesc"].ToString(), 
                PropName = dataRow["PropName"].ToString(), 
                PropValue = dataRow["PropValue"].ToString() 
            };
        }

        private SysDatabaseProp GetDirect(string prop)
        {
            SysDatabaseProp sysDatabaseProp = new SysDatabaseProp();
            using (SqlConnection connection = new SqlConnection(ConnectionString))
            {
                SqlCommand command = new SqlCommand("SELECT TOP(1) * FROM [SysDatabaseProps] WHERE [PropName] = @PropName", connection);
                command.Parameters.AddWithValue("@PropName", prop);
                try
                {
                    connection.Open();
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            sysDatabaseProp.PropDesc = reader["PropDesc"].ToString();
                            sysDatabaseProp.PropName = reader["PropName"].ToString();
                            sysDatabaseProp.PropValue = reader["PropValue"].ToString();
                        }
                        connection.Close();
                    }
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }
            return sysDatabaseProp;
        }
        private SysDatabaseProp GetManual(string prop)
        {
            switch (prop)
            {
                case "DoubleUtils.MidpointRounding":
                    return new SysDatabaseProp()
                    {
                        PropDesc = "Алгоритм округления: 0 - ToEven (к ближайшему чётному), 1 - AwayFromZero (к большему целому), прочие значения интерпретируются как 0.",
                        PropName = "DoubleUtils.MidpointRounding",
                        PropValue = ((int)Rounding).ToString()
                    };
                case "DoubleUtils.NumberDecimalSeparator":
                    return new SysDatabaseProp()
                    {
                        PropDesc = "Разделитель целой и дробной части числа",
                        PropName = "DoubleUtils.NumberDecimalSeparator",
                        PropValue = Separator
                    };
                default:
                    return new SysDatabaseProp();
            }
        }

        public bool CanDeleteMailTemplate(Guid tmplUid)
        {
            throw new NotImplementedException();
        }
        public void DeleteStatusScheme(StatusScheme scheme)
        {
            throw new NotImplementedException();
        }
        public MailTemplate[] GetAllMailTemplates()
        {
            throw new NotImplementedException();
        }
        public List<TTValueFmtTransport> GetAllTTValueFmts()
        {
            throw new NotImplementedException();
        }
        public TechTestAnalog[] GetAnalogTechTests(Guid techUid)
        {
            throw new NotImplementedException();
        }
        public TechTestAnalog[] GetAnalogTechTests(Guid techUid, DateTime timeStamp)
        {
            throw new NotImplementedException();
        }
        public TechTestAnalog[] GetAnalogTechTestsWithAuxilary(Guid techUid)
        {
            throw new NotImplementedException();
        }
        public TechTestAnalog[] GetAnalogTechTestsWithAuxilary(Guid techUid, DateTime timeStamp)
        {
            throw new NotImplementedException();
        }
        public StatusEx[] GetAvailableStatuses(int statusId, int userId)
        {
            throw new NotImplementedException();
        }
        public StatusEx[] GetAvailableStatuses(int statusId, int userId, int[] excludedStatuses)
        {
            throw new NotImplementedException();
        }
        public StatusChangeLog[] GetChangeLogItems(Guid objectUid)
        {
            throw new NotImplementedException();
        }
        public StatusChangeLog[] GetChangeLogItems(Guid objectUid, int statusId)
        {
            throw new NotImplementedException();
        }
        public ClassifierItem[] GetChildClassifierItemByCode(Guid classifierTypeUid, string code)
        {
            throw new NotImplementedException();
        }
        public ClassifierItem[] GetClassifierItem(Guid classifierTypeUid)
        {
            throw new NotImplementedException();
        }
        public ClassifierItem[] GetClassifierItemByCode(Guid classifierTypeUid, string code, bool withChildren, bool withParents)
        {
            throw new NotImplementedException();
        }
        public ClassifierItem GetClassifierItemById(int id)
        {
            throw new NotImplementedException();
        }
        public ClassifierType[] GetClassifierType()
        {
            throw new NotImplementedException();
        }
        public ClassifierType GetClassifierType(Guid classifierTypeUid)
        {
            throw new NotImplementedException();
        }
        public List<UDTField> GetCPUdtFields()
        {
            throw new NotImplementedException();
        }
        public SysDatabaseProp GetDatabaseProp(string prop)
        {
            switch (ConnWorkType)
            {
                case ServiceFactory.WorkType.Direct:
                    return GetDirect(prop);
                case ServiceFactory.WorkType.Manual:
                    return GetManual(prop);
                case ServiceFactory.WorkType.Session:
                default:
                    return GetThroughSession(prop);
            }
        }

        public SysDatabaseProp[] GetDatabaseProps(string[] props)
        {
            throw new NotImplementedException();
        }
        public DigitalSet GetDigitalSet(int digitalSetId)
        {
            throw new NotImplementedException();
        }

        public DigitalSet[] GetDigitalSets()
        {
            throw new NotImplementedException();
        }

        public DigitalSet[] GetDigitalSetsAsync()
        {
            throw new NotImplementedException();
        }

        public DigitalSetValue GetDigitalSetValue(int digitalSetValueId)
        {
            throw new NotImplementedException();
        }

        public DigitalSetValue[] GetDigitalSetValues(int digitalSetId)
        {
            throw new NotImplementedException();
        }

        public DigitalSetValue[] GetDigitalSetValues(int digitalSetId, string value)
        {
            throw new NotImplementedException();
        }

        public List<DigitTechTestResultHistory> GetDigitTechTestResultHistory(Guid productUid, Guid cpUid, Guid techTestUid, DateTime startDate, DateTime stopDate)
        {
            throw new NotImplementedException();
        }

        public List<DigitTechTestResultHistory> GetDigitTechTestResultHistory(Guid? productUid, string product, Guid? cpUid, string cp, Guid? techTestUid, string test, string tech, int? subdivisionId, string subdivision, string ep, string statuses, string batchNum, DateTime startDate, DateTime stopDate)
        {
            throw new NotImplementedException();
        }

        public DateTime GetDpmTime()
        {
            throw new NotImplementedException();
        }

        public EngUnit GetEngUnit(int engUnitId)
        {
            throw new NotImplementedException();
        }

        public EngUnit GetEngUnit(string lAbbr)
        {
            throw new NotImplementedException();
        }

        public EngUnit GetEngUnit(Guid techTestUid, DateTime timeStamp)
        {
            throw new NotImplementedException();
        }

        public EngUnit GetEngUnitAnyString(string engUnit)
        {
            throw new NotImplementedException();
        }

        public EngUnitClass GetEngUnitClass(int engUnitClassId)
        {
            throw new NotImplementedException();
        }

        public EngUnitClass[] GetEngUnitClasses()
        {
            throw new NotImplementedException();
        }

        public EngUnit[] GetEngUnits()
        {
            throw new NotImplementedException();
        }

        public EngUnit[] GetEngUnits(int engUnitClassId)
        {
            throw new NotImplementedException();
        }

        public EngUnit[] GetEngUnits(int[] engUnitIds)
        {
            throw new NotImplementedException();
        }

        public EngUnit[] GetEngUnitsAsync()
        {
            throw new NotImplementedException();
        }

        public int GetFirstRoleId()
        {
            throw new NotImplementedException();
        }

        public StatusScheme[] GetFPShipStatusSchemes()
        {
            throw new NotImplementedException();
        }

        public StatusScheme[] GetFPShipStatusSchemes(int userId)
        {
            throw new NotImplementedException();
        }

        public Group GetGroup(int groupId)
        {
            throw new NotImplementedException();
        }

        public Group GetGroup(string name)
        {
            throw new NotImplementedException();
        }

        public Group GetGroupAS(int groupId)
        {
            throw new NotImplementedException();
        }

        public Group[] GetGroups()
        {
            throw new NotImplementedException();
        }

        public List<IndeterminationModeValue> GetIndeterminationModeValues()
        {
            throw new NotImplementedException();
        }

        public MailTemplate GetMailTemplate(Guid tmplUid)
        {
            throw new NotImplementedException();
        }

        public MaskRow[] GetMasks()
        {
            throw new NotImplementedException();
        }

        public MsgGroupItem[] GetMsgGroupItems(Guid classUid)
        {
            throw new NotImplementedException();
        }

        public MsgGroupItem[] GetMsgGroupItems(Guid classUid, Guid key)
        {
            throw new NotImplementedException();
        }

        public OrgTreeItem[] GetOrgTreeItems()
        {
            throw new NotImplementedException();
        }

        public Period GetPeriod(int periodId)
        {
            throw new NotImplementedException();
        }

        public PeriodType[] GetPeriodTypes(Guid? periodTypeUid, int? lcid)
        {
            throw new NotImplementedException();
        }

        public Priority[] GetPriorities()
        {
            throw new NotImplementedException();
        }

        public Priority GetPriority(int priorityId)
        {
            throw new NotImplementedException();
        }

        public List<UDTField> GetProductUdtFields()
        {
            throw new NotImplementedException();
        }

        public Role GetRole(int roleId)
        {
            throw new NotImplementedException();
        }

        public Role GetRole(string name)
        {
            throw new NotImplementedException();
        }

        public Role[] GetRoles()
        {
            throw new NotImplementedException();
        }

        public Role[] GetRoles(int subdivisionId)
        {
            throw new NotImplementedException();
        }

        public Role[] GetRolesAsync()
        {
            throw new NotImplementedException();
        }

        public string GetSampleAuthrorizeResultsUser(Guid SampleUid)
        {
            throw new NotImplementedException();
        }

        public SampleLog GetSampleLog(int sampleLogId)
        {
            throw new NotImplementedException();
        }

        public SampleLog[] GetSampleLogs()
        {
            throw new NotImplementedException();
        }

        public SampleLog[] GetSampleLogs(SampleLogType sampleLogType)
        {
            throw new NotImplementedException();
        }

        public SampleLog[] GetSampleLogs(SampleLogType sampleLogType, SampleLogState state)
        {
            throw new NotImplementedException();
        }

        public SampleLog[] GetSampleLogs(int userId, SampleLogType sampleLogType, SampleLogState state)
        {
            throw new NotImplementedException();
        }

        public SpecClass GetSpec(Guid specUid)
        {
            throw new NotImplementedException();
        }

        public SpecClass GetSpec(Guid specUid, DateTime timeStamp)
        {
            throw new NotImplementedException();
        }

        public SpecClass[] GetSpecs()
        {
            throw new NotImplementedException();
        }

        public SpecClass[] GetSpecs(DateTime timeStamp)
        {
            throw new NotImplementedException();
        }

        public SpecClass[] GetSpecs(Guid[] uids)
        {
            throw new NotImplementedException();
        }

        public SpecClass[] GetSpecs(Guid[] uids, DateTime timeStamp)
        {
            throw new NotImplementedException();
        }

        public SpecClass[] GetSpecsByProduct(Guid productUid, DateTime timeStamp)
        {
            throw new NotImplementedException();
        }

        public SpecClass[] GetSpecsByProduct(Guid productUid)
        {
            throw new NotImplementedException();
        }

        public SpecClass[] GetSpecsByProductSubdivision(Guid productUid, int[] subIds)
        {
            throw new NotImplementedException();
        }

        public SpecClass[] GetSpecsByProductSubdivision(Guid productUid, int[] subIds, DateTime timeStamp)
        {
            throw new NotImplementedException();
        }

        public Guid[] GetSpecsWithoutOwnTests()
        {
            throw new NotImplementedException();
        }

        public Guid[] GetSpecsWithOwnTests()
        {
            throw new NotImplementedException();
        }

        public int[] GetSpecTechTestVerIds(Guid specUid, DateTime timeStamp)
        {
            throw new NotImplementedException();
        }

        public DateTime GetSqlTime()
        {
            throw new NotImplementedException();
        }

        public Status GetStatus(int statusId)
        {
            throw new NotImplementedException();
        }

        public Status GetStatus(string code)
        {
            throw new NotImplementedException();
        }

        public Status GetStatus(string name, string code)
        {
            throw new NotImplementedException();
        }

        public Status[] GetStatuses()
        {
            throw new NotImplementedException();
        }

        public Status[] GetStatuses(string[] codes)
        {
            throw new NotImplementedException();
        }

        public Status[] GetStatuses(string statusTypeCode)
        {
            throw new NotImplementedException();
        }

        public Status[] GetStatusesAsync()
        {
            throw new NotImplementedException();
        }

        public Status[] GetStatusesAsync(string statusTypeCode)
        {
            throw new NotImplementedException();
        }

        public Status[] GetStatusesForTypes(string[] statusTypeCodes)
        {
            throw new NotImplementedException();
        }

        public Status[] GetStatusesWithTypes()
        {
            throw new NotImplementedException();
        }

        public STCP GetSTCP(Guid stcpUid)
        {
            throw new NotImplementedException();
        }

        public STCP GetSTCP(Guid stcpUid, DateTime timeStamp)
        {
            throw new NotImplementedException();
        }

        public STCP[] GetSTCPs(Guid stUid)
        {
            throw new NotImplementedException();
        }

        public STCP[] GetSTCPs(Guid stUid, DateTime timeStamp)
        {
            throw new NotImplementedException();
        }

        public STCP[] GetSTCPs()
        {
            throw new NotImplementedException();
        }

        public STCP[] GetSTCPs(DateTime timeStamp)
        {
            throw new NotImplementedException();
        }

        public STCP[] GetSTCPs(ObjectState productState, ObjectState cpState, ObjectState stcpState)
        {
            throw new NotImplementedException();
        }

        public STCP[] GetSTCPs(ObjectState productState, ObjectState cpState, ObjectState stcpState, DateTime timeStamp)
        {
            throw new NotImplementedException();
        }

        public Dictionary<Guid, string> GetStdDocNames(Guid[] stdDocUids)
        {
            throw new NotImplementedException();
        }

        public STInfo[] GetSTInfos()
        {
            throw new NotImplementedException();
        }

        public STInfo[] GetSTInfosAnyDate()
        {
            throw new NotImplementedException();
        }

        public STInfo[] GetSTInfosIsCompletedAnyDate()
        {
            throw new NotImplementedException();
        }

        public Subdivision GetSubdivision(int subdivisionId)
        {
            throw new NotImplementedException();
        }

        public Subdivision GetSubdivision(string name, string code)
        {
            throw new NotImplementedException();
        }

        public Subdivision[] GetSubdivisions()
        {
            throw new NotImplementedException();
        }

        public Subdivision[] GetSubdivisionsAsync()
        {
            throw new NotImplementedException();
        }

        public Subdivision[] GetSubdivisionsByIds(int[] subdivisionsIds)
        {
            throw new NotImplementedException();
        }

        public SubdivisionUser[] GetSubdivisionsUsers(int[] userIds)
        {
            throw new NotImplementedException();
        }

        public SysSeq[] GetSysSeq()
        {
            throw new NotImplementedException();
        }

        public SysSeq GetSysSeq(int sysSeqId)
        {
            throw new NotImplementedException();
        }

        public Tech GetTech(Guid techUid)
        {
            throw new NotImplementedException();
        }

        public Tech GetTech(Guid techUid, DateTime timeStamp)
        {
            throw new NotImplementedException();
        }

        public Tech GetTechLastVersion(Guid techUid)
        {
            throw new NotImplementedException();
        }

        public Tech[] GetTechs()
        {
            throw new NotImplementedException();
        }

        public Tech[] GetTechs(DateTime timeStamp)
        {
            throw new NotImplementedException();
        }

        public Tech[] GetTechs(int[] testIds, DateTime timeStamp)
        {
            throw new NotImplementedException();
        }

        public Tech[] GetTechs(Guid productUid, int subdivisionId)
        {
            throw new NotImplementedException();
        }

        public Tech[] GetTechs(Guid productUid, int subdivisionId, DateTime timeStamp)
        {
            throw new NotImplementedException();
        }

        public Tech[] GetTechs(Guid[] excluded)
        {
            throw new NotImplementedException();
        }

        public Tech[] GetTechs(Guid[] excluded, Guid[] included)
        {
            throw new NotImplementedException();
        }

        public Tech[] GetTechs(int? engUnitClassId)
        {
            throw new NotImplementedException();
        }

        public Tech[] GetTechs(DateTime startDate, DateTime stopDate)
        {
            throw new NotImplementedException();
        }

        public Tech[] GetTechs(Guid[] excluded, DateTime startDate, DateTime stopDate)
        {
            throw new NotImplementedException();
        }

        public Tech[] GetTechs(Guid[] excluded, Guid[] included, DateTime startDate, DateTime stopDate)
        {
            throw new NotImplementedException();
        }

        public Tech[] GetTechs(int? engUnitClassId, DateTime startDate, DateTime stopDate)
        {
            throw new NotImplementedException();
        }

        public Tech[] GetTechsByIds(Guid[] techuids)
        {
            throw new NotImplementedException();
        }

        public Tech[] GetTechsByIds(Guid[] techuids, DateTime timestamp)
        {
            throw new NotImplementedException();
        }

        public Tech[] GetTechsBySpecUid(Guid specUid, DateTime timeStamp)
        {
            throw new NotImplementedException();
        }

        public Tech[] GetTechsBySubdivision(int subdivisionId)
        {
            throw new NotImplementedException();
        }

        public Tech[] GetTechsByTestId(int testId)
        {
            throw new NotImplementedException();
        }

        public TechTest GetTechTest(Guid techTestUid)
        {
            throw new NotImplementedException();
        }

        public TechTest GetTechTest(Guid techTestUid, DateTime timeStamp)
        {
            throw new NotImplementedException();
        }

        public TechTest GetTechTest(Guid techUid, int testId)
        {
            throw new NotImplementedException();
        }

        public int? GetTechTestDigit(Guid techTestUid)
        {
            throw new NotImplementedException();
        }

        public EngUnit GetTechTestEngUnit(Guid techTestUid)
        {
            throw new NotImplementedException();
        }

        public EngUnit GetTechTestEngUnit(Guid techTestUid, DateTime timeStamp)
        {
            throw new NotImplementedException();
        }

        public TechTest GetTechTestLastVersion(Guid techTestUid)
        {
            throw new NotImplementedException();
        }

        public TechTestLimit GetTechTestLimit(int techTestVerId)
        {
            throw new NotImplementedException();
        }

        public List<AnalogTechTestResultHistory> GetTechTestResultHistory(Guid productUid, Guid cpUid, Guid techTestUid, DateTime startDate, DateTime stopDate)
        {
            throw new NotImplementedException();
        }

        public List<AnalogTechTestResultHistory> GetTechTestResultHistory(Guid? productUid, string product, Guid? cpUid, string cp, Guid? techTestUid, string test, string tech, int? subdivisionId, string subdivision, string ep, string statuses, string batchNum, DateTime startDate, DateTime stopDate)
        {
            throw new NotImplementedException();
        }

        public TechTest[] GetTechTests(Guid techUid)
        {
            throw new NotImplementedException();
        }

        public TechTest[] GetTechTests(Guid[] techTestUids)
        {
            throw new NotImplementedException();
        }

        public TechTest[] GetTechTests(int[] testIds, DateTime timeStamp)
        {
            throw new NotImplementedException();
        }

        public TechTest[] GetTechTests(Guid techUid, DateTime timeStamp)
        {
            throw new NotImplementedException();
        }

        public TechTest[] GetTechTests(Guid techUid, DateTime startDate, DateTime stopDate)
        {
            throw new NotImplementedException();
        }

        public List<TechTest> GetTechTests(IEnumerable<Guid> techTestUids, DateTime timeStamp)
        {
            throw new NotImplementedException();
        }

        public TechTest[] GetTechTestsByTechTest(Guid[] techTestUids)
        {
            throw new NotImplementedException();
        }

        public TechTestSearchDataSource GetTechTestSearchDataSource(int[] testTypes, DateTime timeStamp, int[] stdDocTypes)
        {
            throw new NotImplementedException();
        }

        public TechTest[] GetTechTestsLastVersion(Guid techUid)
        {
            throw new NotImplementedException();
        }

        public Test GetTest(int testId)
        {
            throw new NotImplementedException();
        }

        public Test GetTest(string name)
        {
            throw new NotImplementedException();
        }

        public Test GetTest(string name, int testType)
        {
            throw new NotImplementedException();
        }

        public Test GetTestByCode(string code)
        {
            throw new NotImplementedException();
        }

        public TestProp GetTestProp(int testId)
        {
            throw new NotImplementedException();
        }

        public TestProp[] GetTestProps()
        {
            throw new NotImplementedException();
        }

        public Test[] GetTests()
        {
            throw new NotImplementedException();
        }

        public Test[] GetTests(int[] testTypes, int[] ignoredList)
        {
            throw new NotImplementedException();
        }

        public Test[] GetTests(int[] testTypes, int[] includedList, int[] ignoredList)
        {
            throw new NotImplementedException();
        }

        public Test[] GetTests(int[] testTypes)
        {
            throw new NotImplementedException();
        }

        public Test[] GetTests(int testType)
        {
            throw new NotImplementedException();
        }

        public Test[] GetTestsAsync()
        {
            throw new NotImplementedException();
        }

        public Test[] GetTestsByTestIds(int[] testIds)
        {
            throw new NotImplementedException();
        }

        public Test[] GetTestsForAllTechs(int[] testTypes, DateTime timeStamp)
        {
            throw new NotImplementedException();
        }

        public ClassifierItem[] GetTopLevelClassifierItems(Guid classifierTypeUid)
        {
            throw new NotImplementedException();
        }

        public List<UDTFieldItem> GetUdtFieldItems(string[] objectTypeNames, List<Guid> userFieldIds, Guid? objectUid)
        {
            throw new NotImplementedException();
        }

        public List<UDTField> GetUdtFields(string[] objectTypeNames)
        {
            throw new NotImplementedException();
        }

        public User GetUser(int userId)
        {
            throw new NotImplementedException();
        }

        public User GetUser(string name)
        {
            throw new NotImplementedException();
        }

        public UserField GetUserField(int userFieldId)
        {
            throw new NotImplementedException();
        }

        public UserField[] GetUserFields()
        {
            throw new NotImplementedException();
        }

        public UserRole[] GetUserRoles(int[] userIds)
        {
            throw new NotImplementedException();
        }

        public User[] GetUsers()
        {
            throw new NotImplementedException();
        }

        public User[] GetUsersForGroup(int groupId)
        {
            throw new NotImplementedException();
        }

        public User[] GetUsersForGroupAS(int groupId)
        {
            throw new NotImplementedException();
        }

        public User[] GetUsersForRole(int roleId)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<User> GetUsersForRoleWithoutServiceFields(int roleId)
        {
            throw new NotImplementedException();
        }

        public User[] GetUsersForSubdivision(int subdivisionId)
        {
            throw new NotImplementedException();
        }

        public VerDesc GetVerDesc(int verId)
        {
            throw new NotImplementedException();
        }

        public VerDesc[] GetVersions(Guid objectUid)
        {
            throw new NotImplementedException();
        }

        public VerDesc[] GetVersions(string objectType)
        {
            throw new NotImplementedException();
        }

        public Dictionary<IndeterminationMode, string> IndeterminationModes_Get(int LCID)
        {
            throw new NotImplementedException();
        }

        public void InsertStatusChangeLogItem(StatusChangeLog item)
        {
            throw new NotImplementedException();
        }

        public void InsertStatusScheme(StatusScheme scheme)
        {
            throw new NotImplementedException();
        }

        public bool InsertTest(ref Test test, out string error)
        {
            throw new NotImplementedException();
        }

        public bool IsExistsEqpForTest(Guid sampleUid, Guid techTestUid, Guid eqpUid)
        {
            throw new NotImplementedException();
        }

        public bool IsExistsProject(Guid techUid)
        {
            throw new NotImplementedException();
        }

        public MaskTemplateRow[] MaskTemplate_Get(int maskType)
        {
            throw new NotImplementedException();
        }

        public MaskRow Mask_Get(int maskId)
        {
            throw new NotImplementedException();
        }

        public string Period_Delete(Period period)
        {
            throw new NotImplementedException();
        }

        public int Period_Insert(Period period)
        {
            throw new NotImplementedException();
        }

        public string Period_Save(ref Period period)
        {
            throw new NotImplementedException();
        }

        public string Period_Update(Period period)
        {
            throw new NotImplementedException();
        }

        public void SaveChangedTTValueFmts(List<TTValueFmtTransport> values)
        {
            throw new NotImplementedException();
        }

        public ClassifierItem[] SearchClassifierItem(Guid classifierTypeUid, string filter)
        {
            throw new NotImplementedException();
        }

        public ClassifierItem[] SearchInChildrenClassifierItem(Guid classifierTypeUid, string code, string filter)
        {
            throw new NotImplementedException();
        }

        public void TestLValueDelete(int testId, int LCID, string value)
        {
            throw new NotImplementedException();
        }

        public void TestLValueInsert(int testId, int LCID, string value)
        {
            throw new NotImplementedException();
        }

        public void TestLValueUpdate(int testId, int LCID, string newValue)
        {
            throw new NotImplementedException();
        }

        public bool TryGetCP(Guid valCPUID, out CPInfo objCP)
        {
            throw new NotImplementedException();
        }

        public bool TryGetProduct(Guid productUID, out RepositoryProductInfo product)
        {
            throw new NotImplementedException();
        }

        public bool UpdateClassifierItem(Guid classifierTypeUid, ClassifierItem[] items, out string error, out DataTable dt)
        {
            throw new NotImplementedException();
        }

        public bool UpdateClassifierType(ClassifierType ct, out string error)
        {
            throw new NotImplementedException();
        }

        public bool UpdateMsgGroupItems(MsgGroupItem[] items, out string error, out MsgGroupItem[] newItems)
        {
            throw new NotImplementedException();
        }

        public void UpdateStatusScheme(StatusScheme scheme)
        {
            throw new NotImplementedException();
        }

        public bool UpdateTemplates(MailTemplate[] templates, out string error, out MailTemplate[] newTemplates)
        {
            throw new NotImplementedException();
        }
    }
}
