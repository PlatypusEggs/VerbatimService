﻿<%@ Page Title="Edit a Deck's Cards" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="DeckCardsView.aspx.cs" Inherits="VerbatimWeb.DeckCardsView" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">
    <asp:HiddenField runat="server" ID="HiddenDeckId"/>

	<br />
	<asp:Label runat="server" Text="Find Card:" Font-Size="Medium"></asp:Label>
	<asp:textbox ID="FilterInputBox" runat="server"></asp:textbox>
	<asp:Button class="btn btn-info" type="button" ID="ButtonFilter" runat="server" onclick="ButtonFilter_Click" Text="Filter" />  
	<br />
	<br />

	<asp:gridview id="DeckCardsGridView" 
		OnRowDataBound="DeckCardsGridView_DataBound"
		OnRowCreated="DeckCardsGridView_RowCreated"
		DeleteMethod="DeleteCard"
		InsertMethod="InsertCard"
  SelectMethod ="LoadDeckCards" 
  autogeneratecolumns="true"
		OnRowUpdating="EditCard"
        UpdateMethod="Test"
  emptydatatext="No cards in the deck!" 
  allowpaging="True" 
 PageSize="10"
		CssClass="grid"
        RowStyle-CssClass="grid-row"
        AlternatingRowStyle-CssClass="grid-row-alternating"
        HeaderStyle-CssClass="grid-header"
        FooterStyle-CssClass="grid-footer"
		AllowSorting="true"
		ShowFooter="false"
		
  runat="server">
    <Columns>
		<asp:CommandField ButtonType="Link" ShowEditButton="True"
     ControlStyle-CssClass="btn btn-warning" />
		
<asp:TemplateField>
  <ItemTemplate>
    <asp:LinkButton runat="server" ID="DeleteButton" 
		ControlStyle-CssClass="btn btn-danger"
      CommandName="delete"  
      OnClientClick="if (!window.confirm('Are you sure you want to delete this item?')) return false;"
		Text="Delete"/>
  </ItemTemplate>
</asp:TemplateField>  
				<%--<asp:CommandField ButtonType="Link" ShowDeleteButton="True"
     ControlStyle-CssClass="btn btn-danger" OnClientClick="return confirm('confirm delete')" />--%>
    </Columns>           
    </asp:GridView>
	<br />
	<h2>Insert Card:</h2>
	<asp:FormView DefaultMode="Insert" ID="InsertCardFormView" runat="server" ItemType="VerbatimService.Card" InsertMethod="InsertCard" >
        <InsertItemTemplate>
            <table>
                <tr>
                    <td>
                        <b>Title</b>:
                    </td>
                    <td>
                        <asp:TextBox ID="Title" runat="server" Columns="100" Text='<%# Bind("Title") %>' />
                        <asp:RequiredFieldValidator ValidationGroup="Insert" ID="RequiredFieldValidator4" runat="server" ControlToValidate="Title" ErrorMessage="*" ForeColor="Red" Font-Size="X-Large"></asp:RequiredFieldValidator>

                    </td>
                </tr>
                <tr>
                    <td>
                        <b>Description</b>:
                    </td>
                    <td>
                        <asp:TextBox ID="Description" runat="server" Columns="100" TextMode="MultiLine" Text='<%# Bind("Description") %>' />
                        <asp:RequiredFieldValidator ValidationGroup="Insert" ID="RequiredFieldValidator1" runat="server" ControlToValidate="Description" ErrorMessage="*" ForeColor="Red" Font-Size="X-Large"></asp:RequiredFieldValidator>

                    </td>
                </tr>
                <tr>
                    <td>
                        <b>Category</b>:
                    </td>
                    <td>
                        <asp:DropDownList ID="DropDownListCategory" runat="server" Autopostback="true" OnSelectedIndexChanged="DropDownListCategory_Changed" Text='<%# Bind("Category") %>'>
										    <%--<asp:ListItem>CELEBRITY</asp:ListItem>
										    <asp:ListItem>FICTIONAL</asp:ListItem>
										    <asp:ListItem>HISTORICAL</asp:ListItem>
										    <asp:ListItem>GEOGRAPHY</asp:ListItem>
										    <asp:ListItem>QUOTE</asp:ListItem>
										    <asp:ListItem>SCIENCE</asp:ListItem>
										    <asp:ListItem>POLITICAL</asp:ListItem>
										    <asp:ListItem>ET CETERA</asp:ListItem>--%>
                                            <asp:ListItem>ADD NEW CATEGORY</asp:ListItem>
								    </asp:DropDownList>
                    </td>
                    <td runat="server" Id="AddNewCategorySection">
                        <b>New Category:</b>
                        <asp:TextBox ID="CustomCategory" runat="server" Text='<%# Bind("CustomCategory") %>'></asp:TextBox>
                    </td>
                </tr>
			    <tr>
                    <td>
                        <b>Point Value</b>:
                    </td>
                    <td>
                        <asp:DropDownList ID="DropDownList" runat="server" Text='<%# Bind("PointValue") %>'>
										    <asp:ListItem>1</asp:ListItem>
										    <asp:ListItem>2</asp:ListItem>
										    <asp:ListItem>3</asp:ListItem>
										    <asp:ListItem>4</asp:ListItem>
                                        </asp:DropDownList>
                    </td>
                </tr>
                <tr>
                    <td>
                        <asp:Button ValidationGroup="Insert" CssClass="btn btn-success" Text="Insert" CausesValidation="True" runat="server" CommandName="Insert" />
                    </td>
                </tr>
            </table>
        </InsertItemTemplate>
        
	</asp:FormView>


</asp:Content>
