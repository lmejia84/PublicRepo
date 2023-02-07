Sub ExportCSV1()
    Dim conn As Object
    Dim strConnection As String, strSQL As String
    Dim filePath As String
    Dim fileName As String
    Dim sourcePath As String
    Dim folderName As String
    Dim srcWorksheet As String
'**********************UPDATE THIS INFO*********************************************************************************************************
    sourcePath = "C:\file.xlsx" 'The path of the file you want to convert to CSV
    srcWorksheet = "Sheet 1" 'This is the Worksheet name, change the name to the same name in your source workbook
    folderName = "C:\"
    fileName = "file.csv"
    filePath = folderName & fileName 'The path where your final data will be saved and ready for GCP"
'***********************************************************************************************************************************************
    Set conn = CreateObject("ADODB.Connection")
    ' This will delete the CSV file if it already exists to avoid errors
    If Dir(filePath) <> "" Then
        Kill filePath
    End If
    ' OPEN DB CONNECTION
    strConnection = "Provider=Microsoft.ACE.OLEDB.12.0;" _
                       & "Data Source=" & sourcePath & ";" _
                       & "Extended Properties=""Excel 8.0;HDR=YES;"";"
    conn.Open strConnection

    ' EXPORT WORKSHEET TO CSV
    strSQL = " SELECT * " _
              & " INTO [text;HDR=Yes;Database=" & folderName & ";" _
              & "CharacterSet=65001]." & fileName _
              & "  FROM [" & srcWorksheet & "$]" '<-- Change This to the name of the Worksheet you want converted to CSV

    ' EXECUTE MAKE-TABLE QUERY
    conn.Execute strSQL
    ' CLOSE CONNECTION
    conn.Close
    Set conn = Nothing
End Sub