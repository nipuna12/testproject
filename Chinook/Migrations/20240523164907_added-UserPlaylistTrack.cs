using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Chinook.Migrations
{
    /// <inheritdoc />
    public partial class addedUserPlaylistTrack : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "UserPlaylistTrack",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    UserId = table.Column<string>(type: "TEXT", nullable: false),
                    PlaylistId = table.Column<long>(type: "INTEGER", nullable: false),
                    TrackId = table.Column<long>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserPlaylistTrack", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserPlaylistTrack_Track_TrackId",
                        column: x => x.TrackId,
                        principalTable: "Track",
                        principalColumn: "TrackId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserPlaylistTrack_UserPlaylists_UserId_PlaylistId",
                        columns: x => new { x.UserId, x.PlaylistId },
                        principalTable: "UserPlaylists",
                        principalColumns: new[] { "UserId", "PlaylistId" },
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_UserPlaylistTrack_TrackId",
                table: "UserPlaylistTrack",
                column: "TrackId");

            migrationBuilder.CreateIndex(
                name: "IX_UserPlaylistTrack_UserId_PlaylistId",
                table: "UserPlaylistTrack",
                columns: new[] { "UserId", "PlaylistId" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserPlaylistTrack");
        }
    }
}
