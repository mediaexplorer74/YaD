using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Ya.D.Migrations
{
    public partial class init : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Items",
                columns: table => new
                {
                    ID = table.Column<uint>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    BigPreviewImage = table.Column<byte[]>(nullable: true),
                    Code = table.Column<int>(nullable: false),
                    ContentLength = table.Column<long>(nullable: false),
                    Created = table.Column<DateTime>(nullable: false),
                    Description = table.Column<string>(nullable: true),
                    DisplayName = table.Column<string>(nullable: true),
                    Error = table.Column<string>(nullable: true),
                    IsFolder = table.Column<bool>(nullable: false),
                    ItemType = table.Column<string>(nullable: true),
                    MimeType = table.Column<string>(nullable: true),
                    Modified = table.Column<DateTime>(nullable: false),
                    ParentFolder = table.Column<string>(nullable: true),
                    Path = table.Column<string>(nullable: true),
                    PreviewImage = table.Column<byte[]>(nullable: true),
                    PreviewURL = table.Column<string>(nullable: true),
                    PublicURL = table.Column<string>(nullable: true),
                    PureResponse = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Items", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "PlayListTypes",
                columns: table => new
                {
                    ID = table.Column<uint>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlayListTypes", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "PlayLists",
                columns: table => new
                {
                    ID = table.Column<uint>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(nullable: true),
                    TypeID = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlayLists", x => x.ID);
                    table.ForeignKey(
                        name: "FK_PlayLists_PlayListTypes_TypeID",
                        column: x => x.TypeID,
                        principalTable: "PlayListTypes",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ItemsInPlaylist",
                columns: table => new
                {
                    ItemID = table.Column<uint>(nullable: false),
                    PlayListID = table.Column<uint>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ItemsInPlaylist", x => new { x.ItemID, x.PlayListID });
                    table.ForeignKey(
                        name: "FK_ItemsInPlaylist_Items_ItemID",
                        column: x => x.ItemID,
                        principalTable: "Items",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ItemsInPlaylist_PlayLists_PlayListID",
                        column: x => x.PlayListID,
                        principalTable: "PlayLists",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ItemsInPlaylist_PlayListID",
                table: "ItemsInPlaylist",
                column: "PlayListID");

            migrationBuilder.CreateIndex(
                name: "IX_PlayLists_TypeID",
                table: "PlayLists",
                column: "TypeID");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ItemsInPlaylist");

            migrationBuilder.DropTable(
                name: "Items");

            migrationBuilder.DropTable(
                name: "PlayLists");

            migrationBuilder.DropTable(
                name: "PlayListTypes");
        }
    }
}
