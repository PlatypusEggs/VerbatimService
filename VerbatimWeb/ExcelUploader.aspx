<%@ Page Title="Upload Deck" Language="C#" AutoEventWireup="true" MasterPageFile="~/Site.Master" CodeBehind="ExcelUploader.aspx.cs" Inherits="VerbatimWeb.ExcelUploader" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">
    <div class="jumbotron">
        <h1>Upload a TSV/CSV!</h1>
        <h2>Make sure you have you columns set up in order: </h2>
        <h2>(Title, Description, Category, Point Value, Picture URL)</h2>
        <h2>Don't add headers, just the data!</h2>
    </div>
    <br />
    <asp:FileUpload accept=".csv*" ID="FileUploadCSV" runat="server" Font-Size="X-Large" Style="max-width:500px" />
    <br />
    <br />
    <br />
    <asp:Button CssClass="btn btn-success" Text="Upload" runat="server" onclick="ButtonUpload_Click" Font-Size="X-Large" />

</asp:Content>
