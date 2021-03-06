﻿Imports System
Imports System.IO
Imports System.Data.SqlClient

Module CB00006FileMove
    '■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■
    '■　コマンド例.  CB00006FileMove /@1 /@2    　　　　　　　　　　　　　　　　　　　      ■
    '■　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　■
    '■　パラメータ説明　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　■
    '■　　・@1：Move元フォルダー　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　■
    '■　　・@2：Move先フォルダー 　　　　　　　　                                           ■
    '■　　・@3：ENEX移行用キーワード（SRVENEX）　                                           ■
    '■　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　■
    '■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■
    Const STATUS_NORMAL As String = "00000"
    Const TABLE_DIRNAME As String = "TABLE"
    Const EXCEL_DIRNAME As String = "EXCEL"
    Const PDF_DIRNAME As String = "PDF"

    Dim WW_DBcon As String = ""
    Dim WW_CopyTo_dir As String = ""

    Sub Main()
        Dim WW_SRVname As String = ""
        Dim WW_cmds_cnt As Integer = 0
        Dim WW_InPARA_FolderFrom As String = ""
        Dim WW_InPARA_FolderTo As String = ""
        Dim WW_InPARA_TERMID As String = ""

        '■■■　共通宣言　■■■
        '*共通関数宣言(BATDLL)
        Dim CS0050DBcon_bat As New BATDLL.CS0050DBcon_bat          'DataBase接続文字取得
        Dim CS0051APSRVname_bat As New BATDLL.CS0051APSRVname_bat  'APサーバ名称取得
        Dim CS0052LOGdir_bat As New BATDLL.CS0052LOGdir_bat        'ログ格納ディレクトリ取得
        Dim CS0054LOGWrite_bat As New BATDLL.CS0054LOGWrite_bat    'LogOutput DirString Get

        '■■■　コマンドライン引数の取得　■■■
        'コマンドライン引数を配列取得
        Dim cmds As String() = System.Environment.GetCommandLineArgs()

        For Each cmd As String In cmds
            Select Case WW_cmds_cnt
                Case 1     'Copy元フォルダー
                    WW_InPARA_FolderFrom = Mid(cmd, 2, 100)
                    Console.WriteLine("引数(Move元　　　  )：" & WW_InPARA_FolderFrom)
                Case 2     'Copy先フォルダー 
                    WW_InPARA_FolderTo = Mid(cmd, 2, 100)
                    Console.WriteLine("引数(Move先　　  　)：" & WW_InPARA_FolderTo)
                Case 3     'ENEX移行用キーワード 
                    WW_InPARA_TERMID = Mid(cmd, 2, 100)
                    Console.WriteLine("引数(ENEX移行時指定)：" & WW_InPARA_TERMID)
            End Select

            WW_cmds_cnt = WW_cmds_cnt + 1
        Next

        '■■■　開始メッセージ　■■■
        CS0054LOGWrite_bat.INFNMSPACE = "CB00006FileMove"               'NameSpace
        CS0054LOGWrite_bat.INFCLASS = "Main"                              'クラス名
        CS0054LOGWrite_bat.INFSUBCLASS = "Main"                           'SUBクラス名
        CS0054LOGWrite_bat.INFPOSI = "CB00006FileMove処理開始"          '
        CS0054LOGWrite_bat.NIWEA = "I"                                    '
        CS0054LOGWrite_bat.TEXT = "CB00006FileMove.exe /" & WW_InPARA_FolderFrom & " /" & WW_InPARA_FolderTo & " /" & WW_InPARA_TERMID & " "
        CS0054LOGWrite_bat.MESSAGENO = "00000"                           'DBエラー
        CS0054LOGWrite_bat.CS0054LOGWrite_bat()                          'ログ入力

        '■■■　共通処理　■■■
        '○ APサーバー名称取得(InParm無し)
        CS0051APSRVname_bat.CS0051APSRVname_bat()
        If CS0051APSRVname_bat.ERR = "00000" Then
            WW_SRVname = Trim(CS0051APSRVname_bat.APSRVname)              'サーバー名格納
        Else
            CS0054LOGWrite_bat.INFNMSPACE = "CB00006FileMove"             'NameSpace
            CS0054LOGWrite_bat.INFCLASS = "Main"                            'クラス名
            CS0054LOGWrite_bat.INFSUBCLASS = "CS0051APSRVname_bat"          'SUBクラス名
            CS0054LOGWrite_bat.INFPOSI = "APサーバー名称取得"
            CS0054LOGWrite_bat.NIWEA = "E"
            CS0054LOGWrite_bat.TEXT = "APサーバー名称取得に失敗（INIファイル設定不備）"
            CS0054LOGWrite_bat.MESSAGENO = CS0051APSRVname_bat.ERR
            CS0054LOGWrite_bat.CS0054LOGWrite_bat()                         'ログ出力
            Environment.Exit(100)
        End If

        '○ ログ格納ディレクトリ取得(InParm無し)
        Dim WW_LOGdir As String = ""
        CS0052LOGdir_bat.CS0052LOGdir_bat()
        If CS0052LOGdir_bat.ERR = "00000" Then
            WW_LOGdir = Trim(CS0052LOGdir_bat.LOGdirStr)                  'ログ格納ディレクトリ格納
        Else
            CS0054LOGWrite_bat.INFNMSPACE = "CB00006FileMove"             'NameSpace
            CS0054LOGWrite_bat.INFCLASS = "Main"                            'クラス名
            CS0054LOGWrite_bat.INFSUBCLASS = "CS0052LOGdir_bat"             'SUBクラス名
            CS0054LOGWrite_bat.INFPOSI = "ログ格納ディレクトリ取得"
            CS0054LOGWrite_bat.NIWEA = "E"
            CS0054LOGWrite_bat.TEXT = "ログ格納ディレクトリ取得に失敗（INIファイル設定不備）"
            CS0054LOGWrite_bat.MESSAGENO = CS0052LOGdir_bat.ERR
            CS0054LOGWrite_bat.CS0054LOGWrite_bat()                         'ログ出力
            Environment.Exit(100)
        End If

        '○ DB接続文字取得(InParm無し)
        CS0050DBcon_bat.CS0050DBcon_bat()
        If CS0050DBcon_bat.ERR = "00000" Then
            WW_DBcon = Trim(CS0050DBcon_bat.DBconStr)                     'DB接続文字格納
        Else
            CS0054LOGWrite_bat.INFNMSPACE = "CB00006FileMove"             'NameSpace
            CS0054LOGWrite_bat.INFCLASS = "Main"                            'クラス名
            CS0054LOGWrite_bat.INFSUBCLASS = "CS0050DBcon_bat"              'SUBクラス名
            CS0054LOGWrite_bat.INFPOSI = "DB接続文字取得"
            CS0054LOGWrite_bat.NIWEA = "E"
            CS0054LOGWrite_bat.TEXT = "DB接続文字取得に失敗（INIファイル設定不備）"
            CS0054LOGWrite_bat.MESSAGENO = CS0050DBcon_bat.ERR
            CS0054LOGWrite_bat.CS0054LOGWrite_bat()                         'ログ出力
            Environment.Exit(100)
        End If

        '■■■　コマンドライン　チェック　■■■
        '○ パラメータチェック(Move元)

        '　自SRVディレクトリのみ可(\\xxxx形式は×)
        If InStr(WW_InPARA_FolderFrom, ":") = 0 Or Mid(WW_InPARA_FolderFrom, 2, 1) <> ":" Then
            CS0054LOGWrite_bat.INFNMSPACE = "CB00006FileMove"             'NameSpace
            CS0054LOGWrite_bat.INFCLASS = "Main"                            'クラス名
            CS0054LOGWrite_bat.INFSUBCLASS = "Main"                         'SUBクラス名
            CS0054LOGWrite_bat.INFPOSI = "引数1チェック"                    '
            CS0054LOGWrite_bat.NIWEA = "E"                                  '
            CS0054LOGWrite_bat.TEXT = "引数1フォーマットエラー：" & WW_InPARA_FolderFrom
            CS0054LOGWrite_bat.MESSAGENO = "00002"                          'パラメータエラー
            CS0054LOGWrite_bat.CS0054LOGWrite_bat()                         'ログ出力
            Environment.Exit(100)
        End If

        '　実在チェック
        If System.IO.Directory.Exists(WW_InPARA_FolderFrom) Then
        Else
            CS0054LOGWrite_bat.INFNMSPACE = "CB00006FileMove"             'NameSpace
            CS0054LOGWrite_bat.INFCLASS = "Main"                            'クラス名
            CS0054LOGWrite_bat.INFSUBCLASS = "Main"                         'SUBクラス名
            CS0054LOGWrite_bat.INFPOSI = "引数1チェック"                    '
            CS0054LOGWrite_bat.NIWEA = "E"                                  '
            CS0054LOGWrite_bat.TEXT = "引数1指定ディレクトリ無し：" & WW_InPARA_FolderFrom
            CS0054LOGWrite_bat.MESSAGENO = "00008"                          'ディレクトリ存在しない
            CS0054LOGWrite_bat.CS0054LOGWrite_bat()                         'ログ出力
            Environment.Exit(100)
        End If

        '○ パラメータチェック(Move先)

        '　自SRVディレクトリのみ可(\\xxxx形式は×)
        If InStr(WW_InPARA_FolderTo, ":") = 0 Or Mid(WW_InPARA_FolderTo, 2, 1) <> ":" Then
            CS0054LOGWrite_bat.INFNMSPACE = "CB00006FileMove"             'NameSpace
            CS0054LOGWrite_bat.INFCLASS = "Main"                            'クラス名
            CS0054LOGWrite_bat.INFSUBCLASS = "Main"                         'SUBクラス名
            CS0054LOGWrite_bat.INFPOSI = "引数2チェック"                    '
            CS0054LOGWrite_bat.NIWEA = "E"                                  '
            CS0054LOGWrite_bat.TEXT = "引数2フォーマットエラー：" & WW_InPARA_FolderTo
            CS0054LOGWrite_bat.MESSAGENO = "00002"                          'パラメータエラー
            CS0054LOGWrite_bat.CS0054LOGWrite_bat()                         'ログ出力
            Environment.Exit(100)
        End If

        '　実在チェック
        If System.IO.Directory.Exists(WW_InPARA_FolderTo) Then
        Else
            Try
                System.IO.Directory.CreateDirectory(WW_InPARA_FolderFrom)
            Catch ex As Exception
                CS0054LOGWrite_bat.INFNMSPACE = "CB00006FileMove"             'NameSpace
                CS0054LOGWrite_bat.INFCLASS = "Main"                            'クラス名
                CS0054LOGWrite_bat.INFSUBCLASS = "Main"                         'SUBクラス名
                CS0054LOGWrite_bat.INFPOSI = "引数2チェック"                    '
                CS0054LOGWrite_bat.NIWEA = "E"                                  '
                CS0054LOGWrite_bat.TEXT = "指定ディレクトリ作成失敗：" & WW_InPARA_FolderTo
                CS0054LOGWrite_bat.MESSAGENO = "00008"                          'ディレクトリ存在しない
                CS0054LOGWrite_bat.CS0054LOGWrite_bat()                         'ログ出力
                Environment.Exit(100)
            End Try
        End If

        '■■■　フォルダ準備　■■■
        'WW_InPARA_FolderFrom（SENDSTOR）を（SENDSTOR_BATCH）に変更
        Dim WW_newFolder As String = ""
        Try
            'ディレクトリ名の変更
            WW_newFolder = WW_InPARA_FolderFrom & "_BATCH"
            If System.IO.Directory.Exists(WW_newFolder) Then
            Else
                System.IO.Directory.Move(WW_InPARA_FolderFrom, WW_newFolder)
                'ディレクトリ名の作成
                If System.IO.Directory.Exists(WW_InPARA_FolderFrom) Then
                Else
                    System.IO.Directory.CreateDirectory(WW_InPARA_FolderFrom)
                End If
            End If
        Catch ex As Exception
            CS0054LOGWrite_bat.INFNMSPACE = "CB00006FileMove"              'NameSpace
            CS0054LOGWrite_bat.INFCLASS = "Main"                            'クラス名
            CS0054LOGWrite_bat.INFSUBCLASS = "Main"                         'SUBクラス名
            CS0054LOGWrite_bat.INFPOSI = "ディレクトリ名変更"                     '
            CS0054LOGWrite_bat.NIWEA = "A"                                  '
            CS0054LOGWrite_bat.TEXT = ex.ToString
            CS0054LOGWrite_bat.MESSAGENO = "00001"                          'パラメータエラー
            CS0054LOGWrite_bat.CS0054LOGWrite_bat()                         'ログ出力
            Environment.Exit(100)
        End Try

        '該当フォルダーよりフォルダー名（＝端末ID）をすべて取得
        Dim WW_dirs As String() = System.IO.Directory.GetDirectories(WW_newFolder, "*")

        'フォルダが存在しない場合、処理を行わない
        If WW_dirs.Count <> 0 Then

            '■■■　データ抽出端末（配信先）一覧を作成　■■■　
            Dim WW_SENDTERMID As New List(Of String)
            Dim WW_TODATATERMID As New List(Of String)

            '■■■　データ抽出端末全て対象　■■■　
            For Each WW_InTermFolder As String In WW_dirs

                WW_CopyTo_dir = Date.Now.ToString("yyyyMMdd") & "_" & Date.Now.ToString("HHmmssfff")

                'フォルダー名より配信元の端末IDを取得
                '例：C:\APPL\APPLFILES\SENDSTOR_BATCH\PCxxxx　→ PCxxxx を取得
                Dim WW_FRDATATERMID As String = System.IO.Path.GetFileName(WW_InTermFolder)
                Dim WW_TblFileArry As String() = System.IO.Directory.GetFiles(WW_InTermFolder, "*.dat", System.IO.SearchOption.AllDirectories)

                For Each WW_TblFile As String In WW_TblFileArry
                    Dim WW_TableID As String = System.IO.Path.GetFileName(WW_TblFile).Replace(".dat", "")

                    '配信先端末ID（配列）を取得
                    WW_TODATATERMID = New List(Of String)
                    GetSendTermArry(WW_SRVname, WW_FRDATATERMID, WW_TableID, WW_InPARA_TERMID, WW_TODATATERMID, WW_SENDTERMID)

                    FileCopy(WW_SENDTERMID, WW_TODATATERMID, WW_TblFile, WW_InPARA_FolderTo, "TABLE")

                Next

                '配信先端末ID（配列）を取得
                WW_TODATATERMID = New List(Of String)
                GetSendTermArry(WW_SRVname, WW_FRDATATERMID, "", WW_InPARA_TERMID, WW_TODATATERMID, WW_SENDTERMID)

                'Excelファイルの振分
                Dim WW_ExcelDir As String = WW_InTermFolder & "\" & EXCEL_DIRNAME
                If System.IO.Directory.Exists(WW_ExcelDir) Then
                    DirCopy(WW_SENDTERMID, WW_TODATATERMID, WW_ExcelDir, WW_InPARA_FolderTo, "EXCEL")
                End If

                'PDFファイルの振分
                Dim WW_PdfDir As String = WW_InTermFolder & "\" & PDF_DIRNAME
                If System.IO.Directory.Exists(WW_PdfDir) Then
                    DirCopy(WW_SENDTERMID, WW_TODATATERMID, WW_PdfDir, WW_InPARA_FolderTo, "PDF")
                End If

            Next
        Else
            Console.WriteLine("対象(コピー元　　)：対象データなし")
            Console.WriteLine("対象(コピー先　　)：対象データなし")
        End If

        '○フォルダーをごと削除
        Try
            If System.IO.Directory.Exists(WW_newFolder) Then
                System.IO.Directory.Delete(WW_newFolder, True)
            End If

        Catch ex As Exception
            CS0054LOGWrite_bat.INFNMSPACE = "CB00006FileMove"              'NameSpace
            CS0054LOGWrite_bat.INFCLASS = "Main"                            'クラス名
            CS0054LOGWrite_bat.INFSUBCLASS = "Main"                         'SUBクラス名
            CS0054LOGWrite_bat.INFPOSI = "ディレクトリ削除"                     '
            CS0054LOGWrite_bat.NIWEA = "A"                                  '
            CS0054LOGWrite_bat.TEXT = ex.ToString
            CS0054LOGWrite_bat.MESSAGENO = "00001"                          'パラメータエラー
            CS0054LOGWrite_bat.CS0054LOGWrite_bat()                         'ログ出力
            Environment.Exit(100)
        End Try

        '■■■　終了メッセージ　■■■
        CS0054LOGWrite_bat.INFNMSPACE = "CB00006FileMove"              'NameSpace
        CS0054LOGWrite_bat.INFCLASS = "Main"                            'クラス名
        CS0054LOGWrite_bat.INFSUBCLASS = "Main"                         'SUBクラス名
        CS0054LOGWrite_bat.INFPOSI = "CB00006FileMove処理終了"                    '
        CS0054LOGWrite_bat.NIWEA = "I"                                  '
        CS0054LOGWrite_bat.TEXT = "CB00006FileMove処理終了"
        CS0054LOGWrite_bat.MESSAGENO = "00000"                          'DBエラー
        CS0054LOGWrite_bat.CS0054LOGWrite_bat()                         'ログ入力
        Environment.Exit(0)

    End Sub

    '-------------------------------------------------------------------------
    '振分先端末ID取得（配列）
    '  概要
    '       指定された端末IDとテーブルIDをもとに振分先端末ID（配列）を取得する
    '　引数
    '　     (IN ) iTermID         : 検索端末ID
    '　     (IN ) iDataTermID     : データ作成端末ID
    '　     (IN ) iTableID        : テーブルID
    '　     (OUT) oToDataTermArry : データ配信先端末ID（配列)
    '　     (OUT) oSernTermArry   : 配信先端末ID（配列)
    '-------------------------------------------------------------------------
    Private Sub GetSendTermArry(ByVal iTermID As String,
                                ByVal iDataTermID As String,
                                ByVal iTableID As String,
                                ByVal iSRVENEX As String,
                                ByRef oToDataTermArry As Object,
                                ByRef oSernTermArry As Object)

        Dim CS0054LOGWrite_bat As New BATDLL.CS0054LOGWrite_bat    'LogOutput DirString Get

        Try
            'DataBase接続文字
            Dim SQLcon As New SqlConnection(WW_DBcon)
            SQLcon.Open() 'DataBase接続(Open)

            Dim SQL_Str As String = ""
            '指定された端末ID、テーブルIDより振分先を取得
            If iTableID = "" Then
                If iSRVENEX = "" Then
                    SQL_Str =
                        " SELECT DISTINCT A.TODATATERMID as TODATATERMID, A.SENDTERMID as SENDTERMID " &
                        " FROM S0018_SENDTERM A " &
                        " INNER JOIN S0027_FTPSERVER B " &
                        " ON    A.SENDTERMID   =    B.SERVERID " &
                        " AND   B.DELFLG       <>   '1' " &
                        " WHERE A.TERMID       =    '" & iTermID & "' " &
                        " AND   A.FRDATATERMID =    '" & iDataTermID & "' " &
                        " AND   A.SENDTERMID   <>   'SRVENEX' " &
                        " AND   A.DELFLG       <>   '1' "
                Else
                    SQL_Str =
                        " SELECT DISTINCT A.TODATATERMID as TODATATERMID, A.SENDTERMID as SENDTERMID " &
                        " FROM S0018_SENDTERM A " &
                        " INNER JOIN S0027_FTPSERVER B " &
                        " ON    A.SENDTERMID   =    B.SERVERID " &
                        " AND   B.DELFLG       <>   '1' " &
                        " WHERE A.TERMID       =    '" & iTermID & "' " &
                        " AND   A.FRDATATERMID =    '" & iDataTermID & "' " &
                        " AND   A.SENDTERMID   like '" & iSRVENEX & "%' " &
                        " AND   A.DELFLG       <>   '1' "
                End If
            Else
                If iSRVENEX = "" Then
                    SQL_Str =
                            " SELECT DISTINCT A.TODATATERMID as TODATATERMID, A.SENDTERMID as SENDTERMID " &
                            " FROM S0018_SENDTERM A " &
                            " INNER JOIN S0027_FTPSERVER B " &
                            " ON    A.SENDTERMID   =    B.SERVERID " &
                            " AND   B.DELFLG       <>   '1' " &
                            " WHERE A.TERMID       =    '" & iTermID & "' " &
                            " AND   A.FRDATATERMID =    '" & iDataTermID & "' " &
                            " AND   A.SENDTERMID   <>   'SRVENEX' " &
                            " AND   A.TBLID        =    '" & iTableID & "' " &
                            " AND   A.DELFLG       <>   '1' "
                Else
                    SQL_Str =
                            " SELECT DISTINCT A.TODATATERMID as TODATATERMID, A.SENDTERMID as SENDTERMID " &
                            " FROM S0018_SENDTERM A " &
                            " INNER JOIN S0027_FTPSERVER B " &
                            " ON    A.SENDTERMID   =    B.SERVERID " &
                            " AND   B.DELFLG       <>   '1' " &
                            " WHERE A.TERMID       =    '" & iTermID & "' " &
                            " AND   A.FRDATATERMID =    '" & iDataTermID & "' " &
                            " AND   A.SENDTERMID   like '" & iSRVENEX & "%' " &
                            " AND   A.TBLID        =    '" & iTableID & "' " &
                            " AND   A.DELFLG       <>   '1' "
                End If
            End If

            Dim SQLcmd As New SqlCommand(SQL_Str, SQLcon)
            Dim SQLdr As SqlDataReader = SQLcmd.ExecuteReader()

            oToDataTermArry.Clear()
            oSernTermArry.Clear()

            While SQLdr.Read
                oToDataTermArry.Add(SQLdr("TODATATERMID"))
                oSernTermArry.Add(SQLdr("SENDTERMID"))
            End While

            'Close
            SQLdr.Close() 'Reader(Close)
            SQLdr = Nothing

            SQLcmd.Dispose()
            SQLcmd = Nothing

            SQLcon.Close() 'DataBase接続(Close)
            SQLcon.Dispose()
            SQLcon = Nothing

        Catch ex As Exception
            CS0054LOGWrite_bat.INFNMSPACE = "CB00006FileMove"               'NameSpace
            CS0054LOGWrite_bat.INFCLASS = "Main"                            'クラス名
            CS0054LOGWrite_bat.INFSUBCLASS = "GetSendTermArry"              'SUBクラス名
            CS0054LOGWrite_bat.INFPOSI = "S0018_SENDTERM SELECT"            '
            CS0054LOGWrite_bat.NIWEA = "A"                                  '
            CS0054LOGWrite_bat.TEXT = ex.ToString
            CS0054LOGWrite_bat.MESSAGENO = "00003"                          'DBエラー
            CS0054LOGWrite_bat.CS0054LOGWrite_bat()                         'ログ出力
            Environment.Exit(100)
        End Try
    End Sub

    '-------------------------------------------------------------------------
    'ファイル振分コピー
    '  概要
    '       指定された端末IDフォルダーにファイルをコピーする
    '　引数
    '       (IN ）iSendTermArry : 配信先端末ID（配列）
    '　     (IN ) iDataTermID   : データ作成端末ID
    '　     (IN ) iFileFrom     : コピー元ファイル名
    '　     (IN ) iSendDir      : コピー先フォルダー
    '　     (IN ) iFileType     : ファイルタイプ
    '-------------------------------------------------------------------------
    Private Sub FileCopy(ByVal iSendTermArry As List(Of String),
                         ByVal iDataTermArry As List(Of String),
                         ByVal iFileFrom As String,
                         ByVal iSendDir As String,
                         ByVal iFileType As String)
        Dim CS0054LOGWrite_bat As New BATDLL.CS0054LOGWrite_bat    'LogOutput DirString Get

        Try
            '○取得した配信先端末IDの数だけファイルをコピーする
            For i As Integer = 0 To iDataTermArry.Count - 1
                '○サブフォルダー作成
                '　(指定フォルダ+送信先PC名)
                Dim WW_TermDir As String = iSendDir & "\" & iSendTermArry(i)
                If System.IO.Directory.Exists(WW_TermDir) Then
                Else
                    System.IO.Directory.CreateDirectory(WW_TermDir)
                End If

                '　(指定フォルダ+送信先PC名+日付時間+(EXCEL or PDF or TABLE)
                Dim WW_TimeDir As String = iSendDir & "\" & iSendTermArry(i) & "\" & WW_CopyTo_dir
                If System.IO.Directory.Exists(WW_TimeDir) Then
                Else
                    System.IO.Directory.CreateDirectory(WW_TimeDir)
                End If

                '　(指定フォルダ+送信先PC名+日付時間+データ作成端末ID
                Dim WW_SendTermDir As String = iSendDir & "\" & iSendTermArry(i) & "\" & WW_CopyTo_dir & "\" & iDataTermArry(i)
                If System.IO.Directory.Exists(WW_SendTermDir) Then
                Else
                    System.IO.Directory.CreateDirectory(WW_SendTermDir)
                End If

                ' ファイルコピー
                Dim WW_FileTo As String = ""
                Select Case iFileType
                    Case "TABLE"
                        WW_FileTo = WW_SendTermDir & "\" & Mid(iFileFrom, iFileFrom.LastIndexOf("TABLE") + 1, 200)
                    Case "EXCEL"
                        WW_FileTo = WW_SendTermDir & "\" & Mid(iFileFrom, iFileFrom.LastIndexOf("EXCEL") + 1, 200)
                    Case "PDF"
                        WW_FileTo = WW_SendTermDir & "\" & Mid(iFileFrom, iFileFrom.LastIndexOf("PDF") + 1, 200)
                End Select
                My.Computer.FileSystem.CopyFile(iFileFrom, WW_FileTo)

                Console.WriteLine("対象(コピー元　　)：" & iFileFrom)
                Console.WriteLine("対象(コピー先　　)：" & WW_FileTo)

            Next

        Catch ex As Exception
            CS0054LOGWrite_bat.INFNMSPACE = "CB00006FileMove"               'NameSpace
            CS0054LOGWrite_bat.INFCLASS = "Main"                            'クラス名
            CS0054LOGWrite_bat.INFSUBCLASS = "FileCopy"                     'SUBクラス名
            CS0054LOGWrite_bat.INFPOSI = "ファイルコピー失敗"               '
            CS0054LOGWrite_bat.NIWEA = "A"                                  '
            CS0054LOGWrite_bat.TEXT = ex.ToString
            CS0054LOGWrite_bat.MESSAGENO = "00001"                          'パラメータエラー
            CS0054LOGWrite_bat.CS0054LOGWrite_bat()                         'ログ出力
            Environment.Exit(100)
        End Try

    End Sub

    '-------------------------------------------------------------------------
    'ファイル振分コピー
    '  概要
    '       指定された端末IDフォルダーをコピーする
    '　引数
    '       (IN ）iSendTermArry : 配信先端末ID（配列）
    '　     (IN ) iDataTermID   : データ作成端末ID
    '　     (IN ) iDirFrom   : コピー元フォルダー名
    '　     (IN ) iDirTo     : コピー先フォルダー
    '　     (IN ) iFileType     : ファイルタイプ
    '-------------------------------------------------------------------------
    Private Sub DirCopy(ByVal iSendTermArry As List(Of String),
                        ByVal iDataTermArry As List(Of String),
                        ByVal iDirFrom As String,
                        ByVal iDirTo As String,
                        ByVal iFileType As String)
        Dim CS0054LOGWrite_bat As New BATDLL.CS0054LOGWrite_bat    'LogOutput DirString Get

        Try
            '○取得した配信先端末IDの数だけファイルをコピーする
            For i As Integer = 0 To iDataTermArry.Count - 1
                '○サブフォルダー作成
                '　(指定フォルダ+送信先PC名)
                Dim WW_TermDir As String = iDirTo & "\" & iSendTermArry(i)
                If System.IO.Directory.Exists(WW_TermDir) Then
                Else
                    System.IO.Directory.CreateDirectory(WW_TermDir)
                End If

                '　(指定フォルダ+送信先PC名+日付時間+(EXCEL or PDF or TABLE)
                Dim WW_TimeDir As String = iDirTo & "\" & iSendTermArry(i) & "\" & WW_CopyTo_dir
                If System.IO.Directory.Exists(WW_TimeDir) Then
                Else
                    System.IO.Directory.CreateDirectory(WW_TimeDir)
                End If

                '　(指定フォルダ+送信先PC名+日付時間+データ作成端末ID
                Dim WW_SendTermDir As String = iDirTo & "\" & iSendTermArry(i) & "\" & WW_CopyTo_dir & "\" & iDataTermArry(i)
                If System.IO.Directory.Exists(WW_SendTermDir) Then
                Else
                    System.IO.Directory.CreateDirectory(WW_SendTermDir)
                End If

                ' ファイルコピー
                Dim WW_FileTo As String = ""
                WW_FileTo = WW_SendTermDir & "\" & iFileType
                My.Computer.FileSystem.CopyDirectory(iDirFrom, WW_FileTo)

                Console.WriteLine("対象(コピー元　　)：" & iDirFrom)
                Console.WriteLine("対象(コピー先　　)：" & WW_FileTo)

            Next

        Catch ex As Exception
            CS0054LOGWrite_bat.INFNMSPACE = "CB00010FileDistribute"         'NameSpace
            CS0054LOGWrite_bat.INFCLASS = "Main"                            'クラス名
            CS0054LOGWrite_bat.INFSUBCLASS = "DirCopy"                     'SUBクラス名
            CS0054LOGWrite_bat.INFPOSI = "フォルダコピー失敗"               '
            CS0054LOGWrite_bat.NIWEA = "A"                                  '
            CS0054LOGWrite_bat.TEXT = ex.ToString
            CS0054LOGWrite_bat.MESSAGENO = "00001"                          'パラメータエラー
            CS0054LOGWrite_bat.CS0054LOGWrite_bat()                         'ログ出力
            Environment.Exit(100)
        End Try

    End Sub


End Module
