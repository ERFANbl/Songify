using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace EntityFrameworkCore.Migrations
{
    /// <inheritdoc />
    public partial class firstdatabasemodelscreated_songPlayListUserPlaylistSong : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SNGF_Song",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Artist = table.Column<string>(type: "text", nullable: true),
                    TrackDuration = table.Column<TimeSpan>(type: "interval", nullable: false),
                    Lyric = table.Column<string>(type: "text", nullable: true),
                    MetaData = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SNGF_Song", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SNGF_User",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "text", nullable: false),
                    MadeForUser = table.Column<string>(type: "text", nullable: true),
                    Token = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SNGF_User", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SNGF_PlayList",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SNGF_PlayList", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SNGF_PlayList_SNGF_User_UserId",
                        column: x => x.UserId,
                        principalTable: "SNGF_User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SNGF_PlaylistSong",
                columns: table => new
                {
                    SongId = table.Column<int>(type: "integer", nullable: false),
                    PlaylistId = table.Column<int>(type: "integer", nullable: false),
                    Position = table.Column<int>(type: "integer", nullable: true),
                    AddedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SNGF_PlaylistSong", x => new { x.PlaylistId, x.SongId });
                    table.ForeignKey(
                        name: "FK_SNGF_PlaylistSong_SNGF_PlayList_PlaylistId",
                        column: x => x.PlaylistId,
                        principalTable: "SNGF_PlayList",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SNGF_PlaylistSong_SNGF_Song_SongId",
                        column: x => x.SongId,
                        principalTable: "SNGF_Song",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SNGF_PlayList_UserId",
                table: "SNGF_PlayList",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_SNGF_PlaylistSong_SongId",
                table: "SNGF_PlaylistSong",
                column: "SongId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SNGF_PlaylistSong");

            migrationBuilder.DropTable(
                name: "SNGF_PlayList");

            migrationBuilder.DropTable(
                name: "SNGF_Song");

            migrationBuilder.DropTable(
                name: "SNGF_User");
        }
    }
}
