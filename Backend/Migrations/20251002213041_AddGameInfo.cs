using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Backend.Migrations
{
    /// <inheritdoc />
    public partial class AddGameInfo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "UserName",
                table: "Users",
                newName: "Username");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "WordHiddens",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(2025, 10, 2, 21, 30, 40, 575, DateTimeKind.Utc).AddTicks(2562),
                oldClrType: typeof(DateTime),
                oldType: "TEXT",
                oldDefaultValue: new DateTime(2025, 9, 24, 22, 20, 49, 156, DateTimeKind.Utc).AddTicks(2136));

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "Votes",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(2025, 10, 2, 21, 30, 40, 575, DateTimeKind.Utc).AddTicks(438),
                oldClrType: typeof(DateTime),
                oldType: "TEXT",
                oldDefaultValue: new DateTime(2025, 9, 24, 22, 20, 49, 155, DateTimeKind.Utc).AddTicks(9935));

            migrationBuilder.AddColumn<int>(
                name: "TotalGamesPlayed",
                table: "Users",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "TotalGamesWon",
                table: "Users",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "TotalScore",
                table: "Users",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "UserName",
                table: "Users",
                type: "TEXT",
                maxLength: 256,
                nullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "StartedAt",
                table: "Rounds",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(2025, 10, 2, 21, 30, 40, 574, DateTimeKind.Utc).AddTicks(8805),
                oldClrType: typeof(DateTime),
                oldType: "TEXT",
                oldDefaultValue: new DateTime(2025, 9, 24, 22, 20, 49, 155, DateTimeKind.Utc).AddTicks(8156));

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "Questions",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(2025, 10, 2, 21, 30, 40, 575, DateTimeKind.Utc).AddTicks(1360),
                oldClrType: typeof(DateTime),
                oldType: "TEXT",
                oldDefaultValue: new DateTime(2025, 9, 24, 22, 20, 49, 156, DateTimeKind.Utc).AddTicks(852));

            migrationBuilder.AlterColumn<DateTime>(
                name: "JoinedAt",
                table: "Players",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(2025, 10, 2, 21, 30, 40, 573, DateTimeKind.Utc).AddTicks(5696),
                oldClrType: typeof(DateTime),
                oldType: "TEXT",
                oldDefaultValue: new DateTime(2025, 9, 24, 22, 20, 49, 154, DateTimeKind.Utc).AddTicks(3799));

            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedAt",
                table: "Games",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(2025, 10, 2, 21, 30, 40, 573, DateTimeKind.Utc).AddTicks(3620),
                oldClrType: typeof(DateTime),
                oldType: "TEXT",
                oldDefaultValue: new DateTime(2025, 9, 24, 22, 20, 49, 154, DateTimeKind.Utc).AddTicks(1328));

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "Games",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(2025, 10, 2, 21, 30, 40, 573, DateTimeKind.Utc).AddTicks(3463),
                oldClrType: typeof(DateTime),
                oldType: "TEXT",
                oldDefaultValue: new DateTime(2025, 9, 24, 22, 20, 49, 154, DateTimeKind.Utc).AddTicks(1167));

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "Answers",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(2025, 10, 2, 21, 30, 40, 574, DateTimeKind.Utc).AddTicks(9980),
                oldClrType: typeof(DateTime),
                oldType: "TEXT",
                oldDefaultValue: new DateTime(2025, 9, 24, 22, 20, 49, 155, DateTimeKind.Utc).AddTicks(9438));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TotalGamesPlayed",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "TotalGamesWon",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "TotalScore",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "UserName",
                table: "Users");

            migrationBuilder.RenameColumn(
                name: "Username",
                table: "Users",
                newName: "UserName");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "WordHiddens",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(2025, 9, 24, 22, 20, 49, 156, DateTimeKind.Utc).AddTicks(2136),
                oldClrType: typeof(DateTime),
                oldType: "TEXT",
                oldDefaultValue: new DateTime(2025, 10, 2, 21, 30, 40, 575, DateTimeKind.Utc).AddTicks(2562));

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "Votes",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(2025, 9, 24, 22, 20, 49, 155, DateTimeKind.Utc).AddTicks(9935),
                oldClrType: typeof(DateTime),
                oldType: "TEXT",
                oldDefaultValue: new DateTime(2025, 10, 2, 21, 30, 40, 575, DateTimeKind.Utc).AddTicks(438));

            migrationBuilder.AlterColumn<DateTime>(
                name: "StartedAt",
                table: "Rounds",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(2025, 9, 24, 22, 20, 49, 155, DateTimeKind.Utc).AddTicks(8156),
                oldClrType: typeof(DateTime),
                oldType: "TEXT",
                oldDefaultValue: new DateTime(2025, 10, 2, 21, 30, 40, 574, DateTimeKind.Utc).AddTicks(8805));

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "Questions",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(2025, 9, 24, 22, 20, 49, 156, DateTimeKind.Utc).AddTicks(852),
                oldClrType: typeof(DateTime),
                oldType: "TEXT",
                oldDefaultValue: new DateTime(2025, 10, 2, 21, 30, 40, 575, DateTimeKind.Utc).AddTicks(1360));

            migrationBuilder.AlterColumn<DateTime>(
                name: "JoinedAt",
                table: "Players",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(2025, 9, 24, 22, 20, 49, 154, DateTimeKind.Utc).AddTicks(3799),
                oldClrType: typeof(DateTime),
                oldType: "TEXT",
                oldDefaultValue: new DateTime(2025, 10, 2, 21, 30, 40, 573, DateTimeKind.Utc).AddTicks(5696));

            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedAt",
                table: "Games",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(2025, 9, 24, 22, 20, 49, 154, DateTimeKind.Utc).AddTicks(1328),
                oldClrType: typeof(DateTime),
                oldType: "TEXT",
                oldDefaultValue: new DateTime(2025, 10, 2, 21, 30, 40, 573, DateTimeKind.Utc).AddTicks(3620));

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "Games",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(2025, 9, 24, 22, 20, 49, 154, DateTimeKind.Utc).AddTicks(1167),
                oldClrType: typeof(DateTime),
                oldType: "TEXT",
                oldDefaultValue: new DateTime(2025, 10, 2, 21, 30, 40, 573, DateTimeKind.Utc).AddTicks(3463));

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "Answers",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(2025, 9, 24, 22, 20, 49, 155, DateTimeKind.Utc).AddTicks(9438),
                oldClrType: typeof(DateTime),
                oldType: "TEXT",
                oldDefaultValue: new DateTime(2025, 10, 2, 21, 30, 40, 574, DateTimeKind.Utc).AddTicks(9980));
        }
    }
}
