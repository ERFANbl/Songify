using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace EntityFrameworkCore.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SNGF_User",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "text", nullable: false),
                    PasswordHash = table.Column<string>(type: "text", nullable: false),
                    MadeForUser = table.Column<string>(type: "text", nullable: true),
                    Token = table.Column<string>(type: "text", nullable: true),
                    FourWeekLogsJson = table.Column<string>(type: "text", nullable: false),
                    UserVectorId = table.Column<string>(type: "text", nullable: false),
                    DateLimit = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SNGF_User", x => x.Id);
                });

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
                    Genre = table.Column<string>(type: "text", nullable: true),
                    ReleaseDate = table.Column<string>(type: "text", nullable: true),
                    ForigenKey = table.Column<string>(type: "text", nullable: true),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false),
                    UserId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SNGF_Song", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SNGF_Song_SNGF_User_UserId",
                        column: x => x.UserId,
                        principalTable: "SNGF_User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SNGF_LikedSongs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    SongId = table.Column<int>(type: "integer", nullable: false),
                    LikedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SNGF_LikedSongs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SNGF_LikedSongs_SNGF_Song_SongId",
                        column: x => x.SongId,
                        principalTable: "SNGF_Song",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_SNGF_LikedSongs_SNGF_User_UserId",
                        column: x => x.UserId,
                        principalTable: "SNGF_User",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_SNGF_LikedSongs_SongId",
                table: "SNGF_LikedSongs",
                column: "SongId");

            migrationBuilder.CreateIndex(
                name: "IX_SNGF_LikedSongs_UserId",
                table: "SNGF_LikedSongs",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_SNGF_Song_UserId",
                table: "SNGF_Song",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SNGF_LikedSongs");

            migrationBuilder.DropTable(
                name: "SNGF_Song");

            migrationBuilder.DropTable(
                name: "SNGF_User");
        }
    }
}
