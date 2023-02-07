'scripts used to get data out of CMS Supervisor R18 and insert into a MS-SQL Server
Module Module1
    Public Sub main()
        Dim cvsApp As Object
        Dim cvsConn As Object
        Dim cvsSrv As Object
        Dim Rep As Object
        Dim serverAddress As String
        Dim UserName As String
        Dim Password1 As String
        Dim Log As Object, b As Object


        'The purpose of this application is to open Avaya in the background only once, and import several reports with one connection.
        'In the past, each script from Avaya was opening one connection and sometimes caused an error for too many instances open.


        cvsApp = CreateObject("ACSUP.cvsApplication")
        cvsSrv = CreateObject("ACSUPSRV.cvsserver")
        Rep = CreateObject("ACSREP.cvsReport")

        serverAddress = "your CMS server name"
        UserName = "your username"
        Password1 = "your password"

        Try
            If cvsApp.CreateServer(UserName, "", "", serverAddress, False, "ENU", cvsSrv, cvsConn) Then

                If cvsConn.Login(UserName, Password1, serverAddress, "ENU") Then
                    Call SummarySkills(cvsSrv, Rep) '<-- Imports Skill Summary
                    Call interval(cvsSrv, Rep) '<-- imports the interval data
                    Call intervalAban(cvsSrv, Rep) '<-- imports the interval by abandon data
                    Call CallReport(cvsSrv, Rep) '<-- imports the agent level data group by skill
                    Call SkillDetail(cvsSrv, Rep)
                End If
                cvsApp.Servers.Remove(cvsSrv.ServerKey)
                cvsConn.Logout
                cvsConn.Disconnect
                cvsSrv.Connected = False

                Log = Nothing
                Rep = Nothing
                cvsSrv = Nothing
                cvsConn = Nothing
                cvsApp = Nothing
            End If
        Catch ex As Exception
            Console.WriteLine("Error Happened, " & ex.Message & " closing program")
            Console.ReadLine()
        End Try

        KillHungProcess("acsApp.exe")
        KillHungProcess("acsRep.exe")
        KillHungProcess("acsSRV.exe")

    End Sub

    Public Sub SummarySkills(cvsSrv As Object, Rep As Object)
        Dim myCN As New ADODB.Connection
        Dim myRS As New ADODB.Recordset
        Dim strSQL As String
        Dim strQuery As String
        Dim Info As Object, Log As Object, b As Object

        On Error Resume Next
        Console.WriteLine("Running Skills Summary from CMS")
        cvsSrv.Reports.ACD = 1
        Info = cvsSrv.Reports.Reports("Historical\Designer\Multiple Skills/Multiple Days (Summary)")

        If Info Is Nothing Then
            If cvsSrv.Interactive Then
                MsgBox("The report Historical\Designer\Multiple Skills/Multiple Days (Summary) was not found on ACD 1.", vbCritical Or vbOKOnly, "Avaya CMS Supervisor")
            Else
                Log = CreateObject("ACSERR.cvsLog")
                Log.AutoLogWrite("The report Historical\Designer\Multiple Skills/Multiple Days (Summary) was not found on ACD 1.")
                Log = Nothing
            End IferName
        Else
            b = cvsSrv.Reports.CreateReport(Info, Rep)
            If b Then
                Rep.Window.Top = 5970
                Rep.Window.Left = 10545
                Rep.Window.Width = 15360
                Rep.Window.Height = 11280
                Rep.TimeZone = "default"
                Rep.SetProperty("Splits/Skills", "1;2;3;339;340;341;342;343;344;345;346;347;348;349;366")
                Rep.SetProperty("Dates", "-1")
                b = Rep.ExportData("C:\Users\lmejia\Documents\Reports\CMS\Multiple Skills Summary\sts.txt", 9, 0, True, True, True)
                Rep.Quit

                If Not cvsSrv.Interactive Then cvsSrv.ActiveTasks.Remove(Rep.TaskID)
                Rep = Nothing
            End If
        End If
        Info = Nothing
        Rep = Nothing
        Log = Nothing

        Console.WriteLine("Inserting Skills Summary into the database...")
        'inserts data to database here
        strSQL = "Provider=sqloledb;Data Source=ServerName;Initial Catalog=STSAnalytics;Integrated Security=SSPI;"
        myCN.Open(strSQL)
        myCN.CommandTimeout = 900
        strQuery = strQuery & vbCrLf & "USE Avaya"
        strQuery = strQuery & vbCrLf & "IF OBJECT_ID('tempdb..##multiple_skills_summary','U') IS NOT NULL DROP TABLE ##multiple_skills_summary"
        strQuery = strQuery & vbCrLf & "CREATE TABLE ##multiple_skills_summary"
        strQuery = strQuery & vbCrLf & "("
        strQuery = strQuery & vbCrLf & "split_skill Varchar(300) NOT NULL,"
        strQuery = strQuery & vbCrLf & "percent_SL Int NOT NULL,"
        strQuery = strQuery & vbCrLf & "asa Decimal(10,2) NOT NULL,"
        strQuery = strQuery & vbCrLf & "aban_time Decimal(10,2) NOT NULL,"
        strQuery = strQuery & vbCrLf & "acd_calls Int NOT NULL,"
        strQuery = strQuery & vbCrLf & "avg_acd_time Decimal(10,2) NOT NULL,"
        strQuery = strQuery & vbCrLf & "avg_acw_time Decimal(10,2) NOT NULL,"
        strQuery = strQuery & vbCrLf & "aban_calls Int NOT NULL,"
        strQuery = strQuery & vbCrLf & "max_delay Int NOT NULL,"
        strQuery = strQuery & vbCrLf & "flow_in Int NOT NULL,"
        strQuery = strQuery & vbCrLf & "flow_out Int NOT NULL,"
        strQuery = strQuery & vbCrLf & "extn_out_calls Int NOT NULL,"
        strQuery = strQuery & vbCrLf & "avg_extn_out_time Decimal(10,2) NOT NULL,"
        strQuery = strQuery & vbCrLf & "percent_acd_time Int NOT NULL,"
        strQuery = strQuery & vbCrLf & "percent_ans_calls Decimal(10,2) NOT NULL"
        strQuery = strQuery & vbCrLf & ")"
        strQuery = strQuery & vbCrLf & ""
        strQuery = strQuery & vbCrLf & ""
        strQuery = strQuery & vbCrLf & "BULK INSERT ##multiple_skills_summary"
        strQuery = strQuery & vbCrLf & ""
        strQuery = strQuery & vbCrLf & "FROM    'C:\Users\lmejia\Documents\Reports\CMS\Multiple Skills Summary\sts.txt'"
        strQuery = strQuery & vbCrLf & "WITH"
        strQuery = strQuery & vbCrLf & "("
        strQuery = strQuery & vbCrLf & "ROWTERMINATOR = '\n',"
        strQuery = strQuery & vbCrLf & "FIELDTERMINATOR = '\t',"
        strQuery = strQuery & vbCrLf & "FIRSTROW = 3"
        strQuery = strQuery & vbCrLf & ")"
        strQuery = strQuery & vbCrLf & ""
        strQuery = strQuery & vbCrLf & "ALTER       TABLE ##multiple_skills_summary"
        strQuery = strQuery & vbCrLf & "ADD         [date] Date"
        strQuery = strQuery & vbCrLf & ""
        strQuery = strQuery & vbCrLf & "UPDATE      ##multiple_skills_summary"
        strQuery = strQuery & vbCrLf & "SET         [date] = Convert(Date,DateAdd(Day,-1,GETDATE()))"
        strQuery = strQuery & vbCrLf & ""
        strQuery = strQuery & vbCrLf & "INSERT INTO avaya.dbo.multiple_skills_summary"
        strQuery = strQuery & vbCrLf & "SELECT      a.*"
        strQuery = strQuery & vbCrLf & ""
        strQuery = strQuery & vbCrLf & "FROM        ##multiple_skills_summary as a"
        strQuery = strQuery & vbCrLf & "WHERE NOT EXISTS"
        strQuery = strQuery & vbCrLf & "            ("
        strQuery = strQuery & vbCrLf & "            select * "
        strQuery = strQuery & vbCrLf & "            from avaya.dbo.multiple_skills_summary as b"
        strQuery = strQuery & vbCrLf & "            where 1 = 1"
        strQuery = strQuery & vbCrLf & "            and a.[date] = b.[date]"
        strQuery = strQuery & vbCrLf & "            and a.split_skill = b.split_skill"
        strQuery = strQuery & vbCrLf & "            and a.percent_SL = b.percent_SL"
        strQuery = strQuery & vbCrLf & "            and a.asa = b.asa"
        strQuery = strQuery & vbCrLf & "            and a.aban_time = b.aban_time"
        strQuery = strQuery & vbCrLf & "            and a.acd_calls = b.acd_calls"
        strQuery = strQuery & vbCrLf & "            and a.avg_acd_time = b.avg_acd_time"
        strQuery = strQuery & vbCrLf & "            and a.avg_acw_time = avg_acw_time"
        strQuery = strQuery & vbCrLf & "            and a.aban_calls = b.aban_calls"
        strQuery = strQuery & vbCrLf & "            and a.max_delay = b.max_delay"
        strQuery = strQuery & vbCrLf & "            and a.extn_out_calls = b.extn_out_calls"
        strQuery = strQuery & vbCrLf & "            and a.avg_extn_out_time = b.avg_extn_out_time"
        strQuery = strQuery & vbCrLf & "            and a.percent_acd_time = b.percent_acd_time"
        strQuery = strQuery & vbCrLf & "            and a.percent_ans_calls = b.percent_ans_calls"
        strQuery = strQuery & vbCrLf & "            )"
        strQuery = strQuery & vbCrLf & "DROP TABLE ##multiple_skills_summary"

        myRS.Open(strQuery, myCN)
        myCN = Nothing
        myCN.Close()

        Kill("C:\Users\lmejia\Documents\Reports\CMS\Multiple Skills Summary\sts.txt")
        Console.WriteLine("Insert complete...")
    End Sub

    Public Sub interval(cvsSrv As Object, Rep As Object)
        Dim mySkill() As Integer = {1, 2, 3, 339, 340, 341, 342, 343, 344, 345, 346, 347, 348, 349, 366}
        Dim Info As Object, Log As Object, b As Object
        Dim mySkill1 As Integer

        On Error Resume Next
        cvsSrv.Reports.ACD = 1

        For Each item In mySkill
            Console.WriteLine("Executing skill " & item)
            Info = cvsSrv.Reports.Reports("Historical\Designer\Call Profile Interval (MultipleSk-MaxD")
            If Info Is Nothing Then
                If cvsSrv.Interactive Then
                    MsgBox("The report Historical\Designer\Call Profile Interval (MultipleSk-MaxD was not found on ACD 1.", vbCritical Or vbOKOnly, "Avaya CMS Supervisor")
                Else
                    Log = CreateObject("ACSERR.cvsLog")
                    Log.AutoLogWrite("The report Historical\Designer\Call Profile Interval (MultipleSk-MaxD was not found on ACD 1.")
                    Log = Nothing
                End If
            Else
                b = cvsSrv.Reports.CreateReport(Info, Rep)
                If b Then
                    Rep.Window.Top = 75
                    Rep.Window.Left = 2272
                    Rep.Window.Width = 15945
                    Rep.Window.Height = 11370
                    Rep.TimeZone = "default"
                    Rep.SetProperty("Splits/Skills", item)
                    Rep.SetProperty("Dates", "-1")
                    Rep.SetProperty("Times", "00:00-23:30")
                    b = Rep.ExportData("C:\Users\lmejia\Documents\Reports\CMS\Call Profile Interval MulSkill MaxD\sts.txt", 9, 0, True, True, True)
                    Rep.Quit
                    If Not cvsSrv.Interactive Then cvsSrv.ActiveTasks.Remove(Rep.TaskID)
                    Rep = Nothing
                End If
            End If
            Info = Nothing
            Rep = Nothing
            Log = Nothing
            mySkill1 = item
            Call BulkInsertInterval(mySkill1) '<--inserts the saved text file into my DB
        Next item
        Console.WriteLine("Interval Data Complete...")
    End Sub

    Public Sub BulkInsertInterval(mySkill1 As Integer)
        Dim myCN As New ADODB.Connection
        Dim myRS As New ADODB.Recordset
        Dim strSQL As String
        Dim strQuery As String

        strSQL = "Provider=sqloledb;Data Source=ServerName;Initial Catalog=STSAnalytics;Integrated Security=SSPI;"
        myCN.Open(strSQL)
        myCN.CommandTimeout = 900
        strQuery = strQuery & vbCrLf & "USE Avaya"
        strQuery = strQuery & vbCrLf & "DECLARE     @myDate Date,"
        strQuery = strQuery & vbCrLf & "            @mySkill Int"
        strQuery = strQuery & vbCrLf & "SET         @myDate = Convert(Date,DateAdd(Day,-1,GETDATE()))"
        strQuery = strQuery & vbCrLf & "SET         @mySkill = " & mySkill1 & ""
        strQuery = strQuery & vbCrLf & ""
        strQuery = strQuery & vbCrLf & "If OBJECT_ID('tempdb..##call_profile_interval_multskill1','U') IS NOT NULL DROP TABLE ##call_profile_interval_multskill"
        strQuery = strQuery & vbCrLf & "CREATE TABLE ##call_profile_interval_multskill"
        strQuery = strQuery & vbCrLf & "("
        strQuery = strQuery & vbCrLf & "    [time_of_day] [time](7) NOT NULL,"
        strQuery = strQuery & vbCrLf & "    [calls_offered] [int] NOT NULL,"
        strQuery = strQuery & vbCrLf & "    [acd_calls] [int] NOT NULL,"
        strQuery = strQuery & vbCrLf & "    [aban_calls] [int] NOT NULL,"
        strQuery = strQuery & vbCrLf & "    [flow_out_calls] [int] NOT NULL,"
        strQuery = strQuery & vbCrLf & "    [percent_ans_calls] [decimal](10, 2) NOT NULL,"
        strQuery = strQuery & vbCrLf & "    sec_30 [decimal](10, 2) NOT NULL,"
        strQuery = strQuery & vbCrLf & "    sec_45[decimal](10, 2) NOT NULL,"
        strQuery = strQuery & vbCrLf & "    sec_60 [decimal](10, 2) NOT NULL,"
        strQuery = strQuery & vbCrLf & "    sec_75 [decimal](10, 2) NOT NULL,"
        strQuery = strQuery & vbCrLf & "    sec_90 [decimal](10, 2) NOT NULL,"
        strQuery = strQuery & vbCrLf & "    sec_105 [decimal](10, 2) NOT NULL,"
        strQuery = strQuery & vbCrLf & "    sec_120 [decimal](10, 2) NOT NULL,"
        strQuery = strQuery & vbCrLf & "    sec_135 [decimal](10, 2) NOT NULL,"
        strQuery = strQuery & vbCrLf & "    sec_150 [decimal](10, 2) NOT NULL,"
        strQuery = strQuery & vbCrLf & "    [asa] [decimal](10, 2) NOT NULL,"
        strQuery = strQuery & vbCrLf & "    [trans_out] [int] NOT NULL,"
        strQuery = strQuery & vbCrLf & "    [max_delay] [decimal](10, 2) NOT NULL"
        strQuery = strQuery & vbCrLf & ")"
        strQuery = strQuery & vbCrLf & ""
        strQuery = strQuery & vbCrLf & "BULK INSERT ##call_profile_interval_multskill"
        strQuery = strQuery & vbCrLf & "FROM    'C:\Users\lmejia\Documents\Reports\CMS\Call Profile Interval MulSkill MaxD\sts.txt'"
        strQuery = strQuery & vbCrLf & "WITH"
        strQuery = strQuery & vbCrLf & "("
        strQuery = strQuery & vbCrLf & "ROWTERMINATOR = '\n',"
        strQuery = strQuery & vbCrLf & "FIELDTERMINATOR = '\t',"
        strQuery = strQuery & vbCrLf & "FIRSTROW = 3"
        strQuery = strQuery & vbCrLf & ")"
        strQuery = strQuery & vbCrLf & "ALTER       TABLE ##call_profile_interval_multskill"
        strQuery = strQuery & vbCrLf & "ADD         [date] Date, split_skill Int"
        strQuery = strQuery & vbCrLf & "UPDATE      ##call_profile_interval_multskill"
        strQuery = strQuery & vbCrLf & "SET         [date] = @myDate, split_skill = @mySkill"
        strQuery = strQuery & vbCrLf & ""
        strQuery = strQuery & vbCrLf & "INSERT INTO avaya.dbo.call_profile_interval_multskill"
        strQuery = strQuery & vbCrLf & "SELECT      "
        strQuery = strQuery & vbCrLf & "            split_skill, [date], time_of_day, calls_offered, acd_calls, aban_calls, flow_out_calls,"
        strQuery = strQuery & vbCrLf & "            percent_ans_calls, sec_30,sec_45,sec_60,sec_75,sec_90,sec_105,sec_120,sec_135,sec_150, asa, trans_out, max_delay"
        strQuery = strQuery & vbCrLf & ""
        strQuery = strQuery & vbCrLf & "FROM        ##call_profile_interval_multskill as a"
        strQuery = strQuery & vbCrLf & "WHERE NOT EXISTS"
        strQuery = strQuery & vbCrLf & "            ("
        strQuery = strQuery & vbCrLf & "            select * "
        strQuery = strQuery & vbCrLf & "            from avaya.dbo.call_profile_interval_multskill as b"
        strQuery = strQuery & vbCrLf & "            where 1 = 1"
        strQuery = strQuery & vbCrLf & "            and a.[date] = b.[date]"
        strQuery = strQuery & vbCrLf & "            and a.split_skill = b.split_skill"
        strQuery = strQuery & vbCrLf & "            and a.time_of_day = b.time_of_day"
        strQuery = strQuery & vbCrLf & "            and a.calls_offered = b.calls_offered"
        strQuery = strQuery & vbCrLf & "            and a.acd_calls = b.acd_calls"
        strQuery = strQuery & vbCrLf & "            and a.aban_calls = b.aban_calls"
        strQuery = strQuery & vbCrLf & "            and a.flow_out_calls = b.flow_out_calls"
        strQuery = strQuery & vbCrLf & "            and a.percent_ans_calls = b.percent_ans_calls"
        strQuery = strQuery & vbCrLf & "            and a.sec_30 = b.sec_30"
        strQuery = strQuery & vbCrLf & "            and a.sec_45 = b.sec_45"
        strQuery = strQuery & vbCrLf & "            and a.sec_60 = b.sec_60"
        strQuery = strQuery & vbCrLf & "            and a.sec_75 = b.sec_75"
        strQuery = strQuery & vbCrLf & "            and a.sec_90 = b.sec_90"
        strQuery = strQuery & vbCrLf & "            and a.sec_105 = b.sec_105"
        strQuery = strQuery & vbCrLf & "            and a.sec_120 = b.sec_120"
        strQuery = strQuery & vbCrLf & "            and a.sec_135 = b.sec_135"
        strQuery = strQuery & vbCrLf & "            and a.sec_150 = b.sec_150"
        strQuery = strQuery & vbCrLf & "            and a.asa = b.asa"
        strQuery = strQuery & vbCrLf & "            and a.trans_out = b.trans_out"
        strQuery = strQuery & vbCrLf & "            and a.max_delay = b.max_delay"
        strQuery = strQuery & vbCrLf & "            )"
        strQuery = strQuery & vbCrLf & ""
        strQuery = strQuery & vbCrLf & "DROP TABLE ##call_profile_interval_multskill;"

        myRS.Open(strQuery, myCN)
        myCN = Nothing
        myCN.Close()

        Kill("C:\Users\lmejia\Documents\Reports\CMS\Call Profile Interval MulSkill MaxD\sts.txt")
        'Console.WriteLine("Skill " & mySkill1 & " has been imported...")
    End Sub

    Public Sub intervalAban(cvsSrv As Object, Rep As Object)
        Dim mySkill() As Integer = {1, 2, 3, 339, 340, 341, 342, 343, 344, 345, 346, 347, 348, 349, 366}
        Dim Info As Object, Log As Object, b As Object
        Dim mySkill1 As Integer

        On Error Resume Next
        cvsSrv.Reports.ACD = 1

        For Each item In mySkill
            Console.WriteLine("Executing by aban skill " & item)
            Info = cvsSrv.Reports.Reports("Historical\Designer\Call Profile Interval Aban by Time")

            If Info Is Nothing Then
                If cvsSrv.Interactive Then
                    MsgBox("The report Historical\Designer\Call Profile Interval Aban by Time was not found on ACD 1.", vbCritical Or vbOKOnly, "Avaya CMS Supervisor")
                Else
                    Log = CreateObject("ACSERR.cvsLog")
                    Log.AutoLogWrite("The report Historical\Designer\Call Profile Interval Aban by Time was not found on ACD 1.")
                    Log = Nothing
                End If
            Else
                b = cvsSrv.Reports.CreateReport(Info, Rep)
                If b Then
                    Rep.Window.Top = -120
                    Rep.Window.Left = -120
                    Rep.Window.Width = 29040
                    Rep.Window.Height = 15840
                    Rep.TimeZone = "default"
                    Rep.SetProperty("Splits/Skills", item)
                    Rep.SetProperty("Dates", "-1")
                    Rep.SetProperty("Times", "00:00-23:30")
                    b = Rep.ExportData("C:\Users\lmejia\Documents\Reports\CMS\Call Profile Interval MulSkill Aban\sts.txt", 9, 0, True, True, True)
                    Rep.Quit
                    If Not cvsSrv.Interactive Then cvsSrv.ActiveTasks.Remove(Rep.TaskID)
                    Rep = Nothing
                End If
            End If
            Info = Nothing
            Rep = Nothing
            Log = Nothing
            mySkill1 = item
            Call BulkInsertIntervalAban(mySkill1)
        Next item
        Console.WriteLine("Interval by aban Data Complete...")
    End Sub

    Public Sub BulkInsertIntervalAban(mySkill1 As Integer)
        Dim myCN As New ADODB.Connection
        Dim myRS As New ADODB.Recordset
        Dim strSQL As String
        Dim strQuery As String

        strSQL = "Provider=sqloledb;Data Source=ServerName;Initial Catalog=STSAnalytics;Integrated Security=SSPI;"
        myCN.Open(strSQL)
        myCN.CommandTimeout = 900

        strQuery = strQuery & vbCrLf & "USE avaya"
        strQuery = strQuery & vbCrLf & "DECLARE     @myDate Date,"
        strQuery = strQuery & vbCrLf & "            @mySkill Int"
        strQuery = strQuery & vbCrLf & "SET         @myDate = Convert(Date,DateAdd(Day,-1,GETDATE()))"
        strQuery = strQuery & vbCrLf & "SET         @mySkill = " & mySkill1 & ""
        strQuery = strQuery & vbCrLf & ""
        strQuery = strQuery & vbCrLf & "IF OBJECT_ID('tempdb..##call_profile_interval_multskill1','U') IS NOT NULL DROP TABLE ##call_profile_interval_multskill"
        strQuery = strQuery & vbCrLf & "CREATE TABLE ##call_profile_interval_multskill"
        strQuery = strQuery & vbCrLf & "("
        strQuery = strQuery & vbCrLf & "    [time_of_day] [time](7) NOT NULL,"
        strQuery = strQuery & vbCrLf & "    [calls_offered] [int] NOT NULL,"
        strQuery = strQuery & vbCrLf & "    [acd_calls] [int] NOT NULL,"
        strQuery = strQuery & vbCrLf & "    [aban_calls] [int] NOT NULL,"
        strQuery = strQuery & vbCrLf & "    [flow_out_calls] [int] NOT NULL,"
        strQuery = strQuery & vbCrLf & "    [percent_ans_calls] [decimal](10, 2) NOT NULL,"
        strQuery = strQuery & vbCrLf & "    sec_30 [decimal](10, 2) NOT NULL,"
        strQuery = strQuery & vbCrLf & "    sec_45[decimal](10, 2) NOT NULL,"
        strQuery = strQuery & vbCrLf & "    sec_60 [decimal](10, 2) NOT NULL,"
        strQuery = strQuery & vbCrLf & "    sec_75 [decimal](10, 2) NOT NULL,"
        strQuery = strQuery & vbCrLf & "    sec_90 [decimal](10, 2) NOT NULL,"
        strQuery = strQuery & vbCrLf & "    sec_105 [decimal](10, 2) NOT NULL,"
        strQuery = strQuery & vbCrLf & "    sec_120 [decimal](10, 2) NOT NULL,"
        strQuery = strQuery & vbCrLf & "    sec_135 [decimal](10, 2) NOT NULL,"
        strQuery = strQuery & vbCrLf & "    sec_150 [decimal](10, 2) NOT NULL,"
        strQuery = strQuery & vbCrLf & "    [asa] [decimal](10, 2) NOT NULL,"
        strQuery = strQuery & vbCrLf & "    [trans_out] [int] NOT NULL,"
        strQuery = strQuery & vbCrLf & "    [max_delay] [decimal](10, 2) NOT NULL"
        strQuery = strQuery & vbCrLf & ")"
        strQuery = strQuery & vbCrLf & ""
        strQuery = strQuery & vbCrLf & "BULK INSERT ##call_profile_interval_multskill"
        strQuery = strQuery & vbCrLf & "FROM    'C:\Users\lmejia\Documents\Reports\CMS\Call Profile Interval MulSkill Aban\sts.txt'"
        strQuery = strQuery & vbCrLf & "WITH"
        strQuery = strQuery & vbCrLf & "("
        strQuery = strQuery & vbCrLf & "ROWTERMINATOR = '\n',"
        strQuery = strQuery & vbCrLf & "FIELDTERMINATOR = '\t',"
        strQuery = strQuery & vbCrLf & "FIRSTROW = 3"
        strQuery = strQuery & vbCrLf & ")"
        strQuery = strQuery & vbCrLf & "ALTER       TABLE ##call_profile_interval_multskill"
        strQuery = strQuery & vbCrLf & "ADD         [date] Date, split_skill Int"
        strQuery = strQuery & vbCrLf & "UPDATE      ##call_profile_interval_multskill"
        strQuery = strQuery & vbCrLf & "SET         [date] = @myDate, split_skill = @mySkill"
        strQuery = strQuery & vbCrLf & ""
        strQuery = strQuery & vbCrLf & "INSERT INTO avaya.dbo.call_profile_interval_multskill_by_aban"
        strQuery = strQuery & vbCrLf & "SELECT      "
        strQuery = strQuery & vbCrLf & "            split_skill, [date], time_of_day, calls_offered, acd_calls, aban_calls, flow_out_calls,"
        strQuery = strQuery & vbCrLf & "            percent_ans_calls, sec_30,sec_45,sec_60,sec_75,sec_90,sec_105,sec_120,sec_135,sec_150, asa, trans_out, max_delay"
        strQuery = strQuery & vbCrLf & ""
        strQuery = strQuery & vbCrLf & "FROM        ##call_profile_interval_multskill as a"
        strQuery = strQuery & vbCrLf & "WHERE NOT EXISTS"
        strQuery = strQuery & vbCrLf & "            ("
        strQuery = strQuery & vbCrLf & "            select * "
        strQuery = strQuery & vbCrLf & "            from avaya.dbo.call_profile_interval_multskill_by_aban as b"
        strQuery = strQuery & vbCrLf & "            where 1 = 1"
        strQuery = strQuery & vbCrLf & "            and a.[date] = b.[date]"
        strQuery = strQuery & vbCrLf & "            and a.split_skill = b.split_skill"
        strQuery = strQuery & vbCrLf & "            and a.time_of_day = b.time_of_day"
        strQuery = strQuery & vbCrLf & "            and a.calls_offered = b.calls_offered"
        strQuery = strQuery & vbCrLf & "            and a.acd_calls = b.acd_calls"
        strQuery = strQuery & vbCrLf & "            and a.aban_calls = b.aban_calls"
        strQuery = strQuery & vbCrLf & "            and a.flow_out_calls = b.flow_out_calls"
        strQuery = strQuery & vbCrLf & "            and a.percent_ans_calls = b.percent_ans_calls"
        strQuery = strQuery & vbCrLf & "            and a.sec_30 = b.sec_30"
        strQuery = strQuery & vbCrLf & "            and a.sec_45 = b.sec_45"
        strQuery = strQuery & vbCrLf & "            and a.sec_60 = b.sec_60"
        strQuery = strQuery & vbCrLf & "            and a.sec_75 = b.sec_75"
        strQuery = strQuery & vbCrLf & "            and a.sec_90 = b.sec_90"
        strQuery = strQuery & vbCrLf & "            and a.sec_105 = b.sec_105"
        strQuery = strQuery & vbCrLf & "            and a.sec_120 = b.sec_120"
        strQuery = strQuery & vbCrLf & "            and a.sec_135 = b.sec_135"
        strQuery = strQuery & vbCrLf & "            and a.sec_150 = b.sec_150"
        strQuery = strQuery & vbCrLf & "            and a.asa = b.asa"
        strQuery = strQuery & vbCrLf & "            and a.trans_out = b.trans_out"
        strQuery = strQuery & vbCrLf & "            and a.max_delay = b.max_delay"
        strQuery = strQuery & vbCrLf & "            )"
        strQuery = strQuery & vbCrLf & ""
        strQuery = strQuery & vbCrLf & "DROP TABLE ##call_profile_interval_multskill;"

        myRS.Open(strQuery, myCN)
        myCN.Close()
        myCN = Nothing

        Kill("C:\Users\lmejia\Documents\Reports\CMS\Call Profile Interval MulSkill Aban\sts.txt")
        'Console.WriteLine("by aban Skill " & mySkill1 & " has been imported...")
    End Sub

    Public Sub CallReport(cvsSrv As Object, Rep As Object)
        Dim myTeam As New List(Of String) From {"POS", "STS"}
        Dim Info As Object, Log As Object, b As Object

        On Error Resume Next


        For Each item In myTeam
            Console.WriteLine("Executing Agent call detail for team: " & item)
            cvsSrv.Reports.ACD = 1
            Info = cvsSrv.Reports.Reports("Historical\Designer\STSAnalytics v4")

            If Info Is Nothing Then
                If cvsSrv.Interactive Then
                    MsgBox("The report Historical\Designer\STSAnalytics v2 was not found on ACD 1.", vbCritical Or vbOKOnly, "Avaya CMS Supervisor")
                Else
                    Log = CreateObject("ACSERR.cvsLog")
                    Log.AutoLogWrite("The report Historical\Designer\STSAnalytics v2 was not found on ACD 1.")
                    Log = Nothing
                End If
            Else
                b = cvsSrv.Reports.CreateReport(Info, Rep)
                If b Then

                    Rep.Window.Top = 1275
                    Rep.Window.Left = 2910
                    Rep.Window.Width = 23070
                    Rep.Window.Height = 13725
                    Rep.TimeZone = "default"
                    Rep.SetProperty("Agent Group", item)
                    Rep.SetProperty("Dates", "-1")
                    b = Rep.ExportData("C:\Users\lmejia\Documents\Reports\CMS\STSAnalytics\pos.txt", 9, 0, True, True, True)
                    Rep.Quit
                    If Not cvsSrv.Interactive Then cvsSrv.ActiveTasks.Remove(Rep.TaskID)
                    Rep = Nothing
                End If

            End If
            Info = Nothing
            Log = Nothing
            Rep = Nothing
            Call SQLcallReport()
        Next item
        Console.WriteLine("Agent call detail complete...")
    End Sub

    Public Sub SQLcallReport()
        Dim myCN As New ADODB.Connection
        Dim myRS As New ADODB.Recordset
        Dim strSQL As String
        Dim strQuery As String


        strSQL = "Provider=sqloledb;Data Source=ServerName;Initial Catalog=STSAnalytics;Integrated Security=SSPI;"
        myCN.Open(strSQL)
        myCN.CommandTimeout = 900

        strQuery = strQuery & vbCrLf & "USE Avaya"
        strQuery = strQuery & vbCrLf & "IF OBJECT_ID('tempdb..##call_report','U') IS NOT NULL DROP TABLE ##call_report"
        strQuery = strQuery & vbCrLf & "CREATE TABLE ##call_report"
        strQuery = strQuery & vbCrLf & "("
        strQuery = strQuery & vbCrLf & "	[date] [date] NOT NULL,"
        strQuery = strQuery & vbCrLf & "	[avaya_emp_no] [int] NOT NULL,"
        strQuery = strQuery & vbCrLf & "	[agent_name] [nvarchar](50) NOT NULL,"
        strQuery = strQuery & vbCrLf & "	[split_skill] [smallint] NOT NULL,"
        strQuery = strQuery & vbCrLf & "	[acd] [nvarchar](50) NOT NULL,"
        strQuery = strQuery & vbCrLf & "	[acd_calls_released] [int] NOT NULL,"
        strQuery = strQuery & vbCrLf & "	[acd_calls] [int] NOT NULL,"
        strQuery = strQuery & vbCrLf & "	[acd_time] [int] NOT NULL,"
        strQuery = strQuery & vbCrLf & "	[acw_time] [int] NOT NULL,"
        strQuery = strQuery & vbCrLf & "	[agent_ring_time] [int] NOT NULL,"
        strQuery = strQuery & vbCrLf & "	[ringtime] [int] NOT NULL,"
        strQuery = strQuery & vbCrLf & "	[abandon_time] [int] NOT NULL,"
        strQuery = strQuery & vbCrLf & "	[held_calls] [int] NOT NULL,"
        strQuery = strQuery & vbCrLf & "	[hold_time] [int] NOT NULL,"
        strQuery = strQuery & vbCrLf & "	[outbound_acd_calls] [int] NOT NULL,"
        strQuery = strQuery & vbCrLf & "	[staffed_time] [int] NOT NULL,"
        strQuery = strQuery & vbCrLf & "	[ti_staffed_time] [int] NOT NULL,"
        strQuery = strQuery & vbCrLf & "	[trans_out] [int] NOT NULL,"
        strQuery = strQuery & vbCrLf & "	[AUX_1] [int] NOT NULL,"
        strQuery = strQuery & vbCrLf & "	[AUX_2] [int] NOT NULL,"
        strQuery = strQuery & vbCrLf & "	[AUX_7] [int] NOT NULL,"
        strQuery = strQuery & vbCrLf & "	[TI_AUXTIME10] [int] NOT NULL,"
        strQuery = strQuery & vbCrLf & "	[TI_AUXTIME11] [int] NOT NULL,"
        strQuery = strQuery & vbCrLf & "	[TI_AUXTIME12] [int] NOT NULL,"
        strQuery = strQuery & vbCrLf & "	[TI_AUXTIME28] [int] NOT NULL,"
        strQuery = strQuery & vbCrLf & "	[avail_time] [int] NOT NULL,"
        strQuery = strQuery & vbCrLf & "	[aban_calls] [int] NOT NULL,"
        strQuery = strQuery & vbCrLf & "	[rona] [int] NULL,"
        strQuery = strQuery & vbCrLf & "	[total_aux_time] [int] NULL,"
        strQuery = strQuery & vbCrLf & "	[holdabancalls] [int] NULL,"
        strQuery = strQuery & vbCrLf & "	[i_acdaux_outtime] [int] NULL,"
        strQuery = strQuery & vbCrLf & "	[i_acdauxintime] [int] NULL,"
        strQuery = strQuery & vbCrLf & "	[loc_id] [int] NULL,"
        strQuery = strQuery & vbCrLf & "	[phantomabns] [int] NULL,"
        strQuery = strQuery & vbCrLf & "	[rejectedintrs] [int] NULL,"
        strQuery = strQuery & vbCrLf & ")"
        strQuery = strQuery & vbCrLf & ""
        strQuery = strQuery & vbCrLf & ""
        strQuery = strQuery & vbCrLf & "BULK INSERT ##call_report"
        strQuery = strQuery & vbCrLf & ""
        strQuery = strQuery & vbCrLf & "FROM	'C:\Users\lmejia\Documents\Reports\CMS\STSAnalytics\pos.txt'"
        strQuery = strQuery & vbCrLf & "WITH"
        strQuery = strQuery & vbCrLf & "("
        strQuery = strQuery & vbCrLf & "ROWTERMINATOR = '\n',"
        strQuery = strQuery & vbCrLf & "FIELDTERMINATOR = '\t',"
        strQuery = strQuery & vbCrLf & "FIRSTROW = 2"
        strQuery = strQuery & vbCrLf & ")"
        strQuery = strQuery & vbCrLf & ""
        strQuery = strQuery & vbCrLf & "INSERT INTO avaya.dbo.call_report"
        strQuery = strQuery & vbCrLf & "SELECT		*"
        strQuery = strQuery & vbCrLf & ""
        strQuery = strQuery & vbCrLf & "FROM		##call_report as a"
        strQuery = strQuery & vbCrLf & "WHERE NOT EXISTS"
        strQuery = strQuery & vbCrLf & "			("
        strQuery = strQuery & vbCrLf & "			select * "
        strQuery = strQuery & vbCrLf & "			from avaya.dbo.call_report as b"
        strQuery = strQuery & vbCrLf & "			where a.[date] = b.[date]"
        strQuery = strQuery & vbCrLf & "			and a.split_skill = b.split_skill"
        strQuery = strQuery & vbCrLf & "			and a.agent_name = b.agent_name"
        strQuery = strQuery & vbCrLf & "			and a.avaya_emp_no = b.avaya_emp_no"
        strQuery = strQuery & vbCrLf & "			and a.acd_calls = b.acd_calls"
        strQuery = strQuery & vbCrLf & "			and a.rona = b.rona"
        strQuery = strQuery & vbCrLf & "			and a.total_aux_time = b.total_aux_time"
        strQuery = strQuery & vbCrLf & "			)"
        strQuery = strQuery & vbCrLf & ""
        strQuery = strQuery & vbCrLf & "DROP TABLE ##call_report"
        myRS.Open(strQuery, myCN)
        myCN = Nothing
        myCN.Close()

        Kill("C:\Users\lmejia\Documents\Reports\CMS\STSAnalytics\pos.txt")
    End Sub

    Public Sub SkillDetail(cvsSrv As Object, Rep As Object)
        Dim Info As Object, Log As Object, b As Object
        Dim myCN As New ADODB.Connection
        Dim myRS As New ADODB.Recordset
        Dim strSQL As String
        Dim strQuery As String

        On Error Resume Next
        Console.WriteLine("Inserting Skill Call Detail")
        cvsSrv.Reports.ACD = 1
        Info = cvsSrv.Reports.Reports("Historical\Designer\STSAnalytics Call Detail v2")

        If Info Is Nothing Then
            If cvsSrv.Interactive Then
                MsgBox("The report Historical\Designer\STSAnalytics Call Detail v2 was not found on ACD 1.", vbCritical Or vbOKOnly, "Avaya CMS Supervisor")
            Else
                Log = CreateObject("ACSERR.cvsLog")
                Log.AutoLogWrite("The report Historical\Designer\STSAnalytics Call Detail v2 was not found on ACD 1.")
                Log = Nothing
            End If
        Else
            b = cvsSrv.Reports.CreateReport(Info, Rep)
            If b Then
                Rep.Window.Top = 1275
                Rep.Window.Left = 3030
                Rep.Window.Width = 22965
                Rep.Window.Height = 13875
                Rep.TimeZone = "default"
                Rep.SetProperty("Date", "-1")
                Rep.ReportView.Add("G0,0,0;-1,2,0", "TABLE0")
                b = Rep.ExportData("C:\Users\lmejia\Documents\Reports\CMS\Call Detail\stsv2.txt", 9, 0, True, True, True)
                Rep.Quit
                If Not cvsSrv.Interactive Then cvsSrv.ActiveTasks.Remove(Rep.TaskID)
                Rep = Nothing
            End If
        End If
        Info = Nothing
        Log = Nothing
        Rep = Nothing

        strSQL = "Provider=sqloledb;Data Source=ServerName;Initial Catalog=STSAnalytics;Integrated Security=SSPI;"
        myCN.Open(strSQL)
        myCN.CommandTimeout = 900
        strQuery = strQuery & vbCrLf & "USE Avaya"
        strQuery = strQuery & vbCrLf & "IF OBJECT_ID('tempdb..##skill_call_detail','u') IS NOT NULL DROP TABLE ##skill_call_detail"
        strQuery = strQuery & vbCrLf & "CREATE TABLE ##skill_call_detail"
        strQuery = strQuery & vbCrLf & "("
        strQuery = strQuery & vbCrLf & "	[Date] [date] NULL,"
        strQuery = strQuery & vbCrLf & "	[Time] [time](7) NULL,"
        strQuery = strQuery & vbCrLf & "	[SEGSTOP] [time](7) NULL,"
        strQuery = strQuery & vbCrLf & "	[QUEUETIME] [varchar](50) NULL,"
        strQuery = strQuery & vbCrLf & "	[Split_Skill] [varchar](50) NULL,"
        strQuery = strQuery & vbCrLf & "	[Ans_Logid] [varchar](50) NULL,"
        strQuery = strQuery & vbCrLf & "	[Calling_Party] [varchar](50) NULL,"
        strQuery = strQuery & vbCrLf & "	[ANSLOCID] [varchar](50) NULL,"
        strQuery = strQuery & vbCrLf & "	[RINGTIME] [varchar](50) NULL,"
        strQuery = strQuery & vbCrLf & "	[Talk_Time] [varchar](50) NULL,"
        strQuery = strQuery & vbCrLf & "	[ACW_Time] [varchar](50) NULL,"
        strQuery = strQuery & vbCrLf & "	[HOLDABN] [varchar](50) NULL,"
        strQuery = strQuery & vbCrLf & "	[HELD] [varchar](50) NULL,"
        strQuery = strQuery & vbCrLf & "	[Trans_Out] [varchar](50) NULL,"
        strQuery = strQuery & vbCrLf & "	[Hold_Time] [varchar](50) NULL,"
        strQuery = strQuery & vbCrLf & "	[AGENTSKILLLEVEL] [varchar](50) NULL,"
        strQuery = strQuery & vbCrLf & "	[AGENTSURPLUS] [varchar](50) NULL,"
        strQuery = strQuery & vbCrLf & "	[Rls] [varchar](50) NULL,"
        strQuery = strQuery & vbCrLf & "	[ANSREASON] [varchar](50) NULL,"
        strQuery = strQuery & vbCrLf & "	[CONSULTTIME] [varchar](50) NULL,"
        strQuery = strQuery & vbCrLf & "	[Dialed_Number] [varchar](50) NULL,"
        strQuery = strQuery & vbCrLf & "	[Disposition] [varchar](50) NULL,"
        strQuery = strQuery & vbCrLf & "	[DISPPRIORITY] [varchar](50) NULL,"
        strQuery = strQuery & vbCrLf & "	[DISPSKLEVEL] [varchar](50) NULL,"
        strQuery = strQuery & vbCrLf & "	[Disposition_Time] [varchar](50) NULL,"
        strQuery = strQuery & vbCrLf & "	[FIRSTIVECTOR] [varchar](50) NULL,"
        strQuery = strQuery & vbCrLf & "	[FIRSTVDN] [varchar](50) NULL,"
        strQuery = strQuery & vbCrLf & "	[LASTDIGITS] [varchar](50) NULL,"
        strQuery = strQuery & vbCrLf & "	[ORIGHOLDTIME] [varchar](50) NULL,"
        strQuery = strQuery & vbCrLf & "	[ORIGLOCID] [varchar](50) NULL,"
        strQuery = strQuery & vbCrLf & "	[ORIGLOGIN] [varchar](50) NULL,"
        strQuery = strQuery & vbCrLf & "	[ORIGREASON] [varchar](50) NULL,"
        strQuery = strQuery & vbCrLf & "	[SPLIT1] [varchar](50) NULL,"
        strQuery = strQuery & vbCrLf & "	[SPLIT2] [varchar](50) NULL,"
        strQuery = strQuery & vbCrLf & "	[SPLIT3] [varchar](50) NULL"
        strQuery = strQuery & vbCrLf & ")"
        strQuery = strQuery & vbCrLf & ""
        strQuery = strQuery & vbCrLf & "BULK INSERT ##skill_call_detail"
        strQuery = strQuery & vbCrLf & "FROM 'C:\Users\lmejia\Documents\Reports\CMS\Call Detail\stsv2.txt'"
        strQuery = strQuery & vbCrLf & "WITH("
        strQuery = strQuery & vbCrLf & "ROWTERMINATOR = '\n',"
        strQuery = strQuery & vbCrLf & "FIELDTERMINATOR = '\t',"
        strQuery = strQuery & vbCrLf & "FIRSTROW = 2"
        strQuery = strQuery & vbCrLf & "	)"
        strQuery = strQuery & vbCrLf & ""
        strQuery = strQuery & vbCrLf & ""
        strQuery = strQuery & vbCrLf & "INSERT INTO avaya.dbo.skill_call_detail"
        strQuery = strQuery & vbCrLf & "SELECT		*"
        strQuery = strQuery & vbCrLf & "FROM		##skill_call_detail a"
        strQuery = strQuery & vbCrLf & "WHERE NOT EXISTS("
        strQuery = strQuery & vbCrLf & "				select *"
        strQuery = strQuery & vbCrLf & "				from avaya.dbo.skill_call_detail b"
        strQuery = strQuery & vbCrLf & "				where a.[date] = b.[row_date]"
        strQuery = strQuery & vbCrLf & "				and a.[Time] = b.[row_time]"
        strQuery = strQuery & vbCrLf & "				and a.Split_Skill = b.split_skill"
        strQuery = strQuery & vbCrLf & "				)"
        myRS.Open(strQuery, myCN)
        myCN = Nothing
        myCN.Close()

        Kill("C:\Users\lmejia\Documents\Reports\CMS\Call Detail\stsv2.txt")
        Console.WriteLine("Skill Call Detail Complete...")
    End Sub

    Public Sub KillHungProcess(processName As String)
        Dim psi As ProcessStartInfo = New ProcessStartInfo
        psi.Arguments = "/im " & processName & " /f"
        psi.FileName = "taskkill"
        Dim p As Process = New Process()
        p.StartInfo = psi
        p.Start()
    End Sub
End Module