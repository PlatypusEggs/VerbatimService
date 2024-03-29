﻿<%@ Page  Title="Create a Deck" Language="C#" AutoEventWireup="true" MasterPageFile="~/Site.Master" CodeBehind="CreateDeck.aspx.cs" Inherits="VerbatimWeb.CreateDeck" validateRequest="false" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">

    <div class="jumbotron">
        <h1>Create a Deck</h1>
    </div>
    <asp:FormView DefaultMode="Insert" ID="CreateDeckFormView" runat="server" ItemType="VerbatimService.Deck" InsertMethod="InsertDeck" >
        <InsertItemTemplate>
            <table>
                <tr>
                    <td style="vertical-align:top">
                        <b>Deck Name</b>:
                    </td>
                    <td>
                        <asp:TextBox ID="Name" runat="server" Columns="100" Text='<%# Bind("Name") %>' />
                        <asp:RequiredFieldValidator ID="RequiredFieldValidator4" runat="server" ControlToValidate="Name" ErrorMessage="*" ForeColor="Red" Font-Size="X-Large"></asp:RequiredFieldValidator>
                    </td>
                </tr>
                <tr>
                    <td style="vertical-align:top">
                        <b>Description</b>:
                    </td>
                    <td>
                        <asp:TextBox ID="Description" runat="server" Columns="100" TextMode="MultiLine" Text='<%# Bind("Description") %>' />
                        <asp:RequiredFieldValidator ID="RequiredFieldValidator1" runat="server" ControlToValidate="Description" ErrorMessage="*" ForeColor="Red" Font-Size="X-Large"></asp:RequiredFieldValidator>
                    </td>
                </tr>
                <tr>
                    <td style="vertical-align:top">
                        <b>Author</b>:
                    </td>
                    <td>
                        <asp:TextBox ID="TextBoxAuthor" runat="server" Columns="100" Text='<%# Bind("Author") %>' />
                        <asp:RequiredFieldValidator ID="RequiredFieldValidator2" runat="server" ControlToValidate="TextBoxAuthor" ErrorMessage="*" ForeColor="Red" Font-Size="X-Large"></asp:RequiredFieldValidator>
                    </td>
                </tr>
                <tr>
                    <td style="vertical-align:top">
                        <b>Language</b>:
                    </td>
                    <td>
                        <asp:TextBox ID="TextBoxLanguage" runat="server" Columns="100" Text='<%# Bind("Language") %>' />
                        <asp:RequiredFieldValidator ID="RequiredFieldValidator5" runat="server" ControlToValidate="TextBoxAuthor" ErrorMessage="*" ForeColor="Red" Font-Size="X-Large"></asp:RequiredFieldValidator>
                    </td>
                </tr>
			    <tr>
                    <td style="vertical-align:top">
                        <b>Token to Load on TTS</b>:
                    </td>
                    <td>
					    <asp:TextBox ID="TextBoxIDToken" runat="server" Columns="100" Text='<%# Bind("IdentifiyngToken") %>' />
                        <asp:RequiredFieldValidator ID="RequiredFieldValidator3" runat="server" ControlToValidate="TextBoxIDToken" ErrorMessage="*" ForeColor="Red" Font-Size="X-Large"></asp:RequiredFieldValidator>
                    </td>
                </tr>
                <tr>
                    <td style="vertical-align:top">
                        <b>Point Distribution</b>:
                    </td>
                    <td>
                        <asp:RadioButtonList runat="server" ID="RadioDistribution" SelectedValue='<%# Bind("MyRblField") %>'>
                            <asp:ListItem Value="false" Text="Random" />
                            <asp:ListItem Value="true" Text="Standard" />
                        </asp:RadioButtonList>
                        <asp:RequiredFieldValidator ID="RequiredFieldValidator6" runat="server" ControlToValidate="RadioDistribution" ErrorMessage="*" ForeColor="Red" Font-Size="X-Large"></asp:RequiredFieldValidator>
                    </td>
                    <td>
                        The Standard distribution will generate play decks with point values with these percentages: 25%, 25%, 40%, 10%. <br />
                        If you plan to create a deck that has cards with these point values distributed in roughly these percentages, choose Standard. <br />
                        If you want the play decks that are generated to pull random cards from the entire deck, choose Random. <br />
                        <b>If you aren't sure, choose Random.</b>
                    </td>
                </tr>
                <tr>
                    <td>
                        <asp:Button CssClass="btn btn-success" Text="Create" runat="server" CausesValidation="True" CommandName="Insert" />
                    </td>
                </tr>
            </table>
        </InsertItemTemplate>
	</asp:FormView>
</asp:Content>
