' APK Resizer by galaxyfreak [http://forum.xda-developers.com/member.php?u=4635923]
' Source code at: github.com/galaxyfreak
'________________________________________________________________________________________

' I know this code is a sloppy and mess but it works :P

Imports ComponentAce.Compression.ZipForge
Imports ComponentAce.Compression.Archiver
Imports System.IO
Imports System.Collections
Imports System.Drawing
Imports System.Drawing.Drawing2D

Public Class Form1
    Dim archiver As New ZipForge()
    Dim APKopen As New OpenFileDialog
    Dim temppath As String = "C:\temp"
    Dim highestres As String
    Dim targetres As String
    Dim highestreslevel As Integer
    Dim targetreslevel As Integer
    Dim OriginalHeight As Single
    Dim OriginalWidth As Single
    Dim NewWidth As Integer
    Dim NewHeight As Integer

    Private Sub Button1_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button1.Click
        ExtractArchive(TextBox1.Text)
        If Label3.Text.Contains("XHDPI") Then
            highestres = "xhdpi"
            highestreslevel = 4
        ElseIf Label3.Text.Contains("HDPI") Then
            highestres = "hdpi"
            highestreslevel = 3
        ElseIf Label3.Text.Contains("MDPI") Then
            highestres = "mdpi"
            highestreslevel = 2
        ElseIf Label3.Text.Contains("LDPI") Then
            highestres = "ldpi"
            highestreslevel = 1
        End If

        If RadioButton1.Checked = True Then
            targetres = "xhdpi"
            targetreslevel = 4
        ElseIf RadioButton2.Checked = True Then
            targetres = "hdpi"
            targetreslevel = 3
        ElseIf RadioButton3.Checked = True Then
            targetres = "mdpi"
            targetreslevel = 2
        ElseIf RadioButton4.Checked = True Then
            targetres = "ldpi"
            targetreslevel = 1
        End If
        If targetreslevel > highestreslevel Then
            MsgBox("Target resolution is higher than original resolution." & vbNewLine & "This can cause pixelated images." & vbNewLine & "Are you sure you want to continue?", MsgBoxStyle.YesNo, "Warning!")
        End If
        ResizeImagesInFolder(temppath & "\res\drawable-" & highestres & "\", temppath & "\res\drawable-" & targetres & "\")
        ZipAPK()
    End Sub

    Private Sub ExtractArchive(ByVal ExtractPath As String)
        Try
            archiver.FileName = TextBox1.Text
            archiver.OpenArchive(System.IO.FileMode.Open)
            archiver.BaseDir = ExtractPath
            archiver.ExtractFiles("*.*")
            archiver.CloseArchive()
        Catch ex As ArchiverException

        End Try
    End Sub

    Private Sub Button2_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button2.Click
        Dim result As DialogResult = APKopen.ShowDialog()
        If result = Windows.Forms.DialogResult.OK Then
            TextBox1.Text = APKopen.FileName
            Label3.Text = "Existing" & vbNewLine & "resolutions:"
            ExtractArchive(temppath)
            RadioButton1.Enabled = True
            RadioButton2.Enabled = True
            RadioButton3.Enabled = True
            RadioButton4.Enabled = True
            If Directory.Exists(temppath & "\res\drawable-xhdpi") Then
                Label3.Text = Label3.Text & " | XHDPI"
                RadioButton1.Enabled = False
            End If
            If Directory.Exists(temppath & "\res\drawable-hdpi") Then
                Label3.Text = Label3.Text & " | HDPI"
                RadioButton2.Enabled = False
            End If
            If Directory.Exists(temppath & "\res\drawable-mdpi") Then
                Label3.Text = Label3.Text & " | MDPI"
                RadioButton3.Enabled = False
            End If
            If Directory.Exists(temppath & "\res\drawable-ldpi") Then
                Label3.Text = Label3.Text & " | LDPI"
                RadioButton4.Enabled = False
            End If
        End If
    End Sub

    Private Sub Button3_Click(ByVal sender As System.Object, ByVal e As System.EventArgs)
        Dim diag As New FolderBrowserDialog()
        diag.Description = "Please select a folder to extract the current archive to."
        diag.RootFolder = Environment.SpecialFolder.MyComputer
        diag.ShowNewFolderButton = True

        Dim result As DialogResult = diag.ShowDialog()
        If result = Windows.Forms.DialogResult.OK Then
            TextBox1.Text = diag.SelectedPath
        End If
    End Sub



    Public Sub ResizeImagesInFolder(ByVal SourceFolder As String, ByVal DestinationFolder As String)
        On Error Resume Next ' Just to prevent ugly error from showing up :P
        If Not Directory.Exists(SourceFolder) Then
            Throw New Exception("SourceFolder does not exist")
        End If
        If Not Directory.Exists(DestinationFolder) Then
            Directory.CreateDirectory(DestinationFolder)
        End If
        Dim diImages As DirectoryInfo = New DirectoryInfo(SourceFolder)
        Dim alImages As ArrayList = New ArrayList()
        alImages.AddRange(diImages.GetFiles("*.png"))
        alImages.AddRange(diImages.GetFiles("*.9.png"))

        Dim imgOriginal As Image
        Dim ResizedBitmap As Bitmap
        Dim ResizedImage As Graphics

        For Each fiImage As FileInfo In alImages
            imgOriginal = Image.FromFile(fiImage.FullName)
            OriginalHeight = imgOriginal.Height
            OriginalWidth = imgOriginal.Width
            CheckSize()
            ResizedBitmap = New Bitmap(NewWidth, NewHeight)
            ResizedImage = Graphics.FromImage(ResizedBitmap)
            ResizedImage.InterpolationMode = InterpolationMode.HighQualityBicubic
            ResizedImage.CompositingQuality = CompositingQuality.HighQuality
            ResizedImage.SmoothingMode = SmoothingMode.HighQuality
            ResizedImage.DrawImage(imgOriginal, 0, 0, NewWidth, NewHeight)
            ResizedBitmap.Save(DestinationFolder + fiImage.Name)
            imgOriginal.Dispose()
            ResizedBitmap.Dispose()
            ResizedImage.Dispose()
        Next
    End Sub

    Sub ZipAPK()
        Dim apkpath As String
        Try
            apkpath = System.IO.Path.GetDirectoryName(APKopen.FileName)
            archiver.FileName = apkpath & "\resized_" & APKopen.SafeFileName
            archiver.OpenArchive(System.IO.FileMode.Create)
            archiver.BaseDir = temppath
            archiver.AddFiles("*.*")
            archiver.CloseArchive()
            Directory.Delete("C:\temp", True)
        Catch ex As ArchiverException

        End Try
    End Sub

    Sub CheckSize()

        If highestres = "xhdpi" And targetres = "hdpi" Then
            NewHeight = OriginalHeight - (OriginalHeight * 0.666)
            NewWidth = OriginalWidth - (OriginalWidth * 0.666)
        End If

        If highestres = "xhdpi" And targetres = "mdpi" Then
            NewHeight = OriginalHeight - (OriginalHeight * 0.444)
            NewWidth = OriginalWidth - (OriginalWidth * 0.444)
        End If

        If highestres = "xhdpi" And targetres = "ldpi" Then
            NewHeight = OriginalHeight - (OriginalHeight * 2.666)
            NewWidth = OriginalWidth - (OriginalWidth * 2.666)
        End If

        If highestres = "hdpi" And targetres = "xhdpi" Then
            NewHeight = OriginalHeight + (OriginalHeight * 1.5)
            NewWidth = OriginalWidth + (OriginalWidth * 1.5)
        End If

        If highestres = "hdpi" And targetres = "mdpi" Then
            NewHeight = OriginalHeight - (OriginalHeight * 0.666)
            NewWidth = OriginalWidth - (OriginalWidth * 0.666)
        End If

        If highestres = "hdpi" And targetres = "ldpi" Then
            NewHeight = OriginalHeight - (OriginalHeight * 2)
            NewWidth = OriginalWidth - (OriginalWidth * 2)
        End If

        If highestres = "mdpi" And targetres = "xhdpi" Then
            NewHeight = OriginalHeight + (OriginalHeight * 2.25)
            NewWidth = OriginalWidth + (OriginalWidth * 2.25)
        End If

        If highestres = "mdpi" And targetres = "hdpi" Then
            NewHeight = OriginalHeight + (OriginalHeight * 1.5)
            NewWidth = OriginalWidth + (OriginalWidth * 1.5)
        End If

        If highestres = "mdpi" And targetres = "ldpi" Then
            NewHeight = OriginalHeight - (OriginalHeight * 1.333)
            NewWidth = OriginalWidth - (OriginalWidth * 1.333)
        End If

        If highestres = "ldpi" And targetres = "xdpi" Then
            NewHeight = OriginalHeight + (OriginalHeight * 2.666)
            NewWidth = OriginalWidth + (OriginalWidth * 2.666)
        End If

        If highestres = "ldpi" And targetres = "hdpi" Then
            NewHeight = OriginalHeight + (OriginalHeight * 2)
            NewWidth = OriginalWidth + (OriginalWidth * 2)
        End If
        If highestres = "ldpi" And targetres = "mdpi" Then
            NewHeight = OriginalHeight + (OriginalHeight * 1.333)
            NewWidth = OriginalWidth + (OriginalWidth * 1.333)
        End If
    End Sub
End Class