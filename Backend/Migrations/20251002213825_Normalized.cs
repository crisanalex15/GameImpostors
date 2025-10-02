using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Backend.Migrations
{
    /// <inheritdoc />
    public partial class Normalized : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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
                defaultValue: new DateTime(2025, 10, 2, 21, 38, 24, 996, DateTimeKind.Utc).AddTicks(6308),
                oldClrType: typeof(DateTime),
                oldType: "TEXT",
                oldDefaultValue: new DateTime(2025, 10, 2, 21, 30, 40, 575, DateTimeKind.Utc).AddTicks(2562));

            migrationBuilder.AddColumn<string>(
                name: "NormalizedWord",
                table: "WordHiddens",
                type: "TEXT",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "Votes",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(2025, 10, 2, 21, 38, 24, 996, DateTimeKind.Utc).AddTicks(4224),
                oldClrType: typeof(DateTime),
                oldType: "TEXT",
                oldDefaultValue: new DateTime(2025, 10, 2, 21, 30, 40, 575, DateTimeKind.Utc).AddTicks(438));

            migrationBuilder.AlterColumn<DateTime>(
                name: "StartedAt",
                table: "Rounds",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(2025, 10, 2, 21, 38, 24, 996, DateTimeKind.Utc).AddTicks(2524),
                oldClrType: typeof(DateTime),
                oldType: "TEXT",
                oldDefaultValue: new DateTime(2025, 10, 2, 21, 30, 40, 574, DateTimeKind.Utc).AddTicks(8805));

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "Questions",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(2025, 10, 2, 21, 38, 24, 996, DateTimeKind.Utc).AddTicks(5128),
                oldClrType: typeof(DateTime),
                oldType: "TEXT",
                oldDefaultValue: new DateTime(2025, 10, 2, 21, 30, 40, 575, DateTimeKind.Utc).AddTicks(1360));

            migrationBuilder.AlterColumn<DateTime>(
                name: "JoinedAt",
                table: "Players",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(2025, 10, 2, 21, 38, 24, 994, DateTimeKind.Utc).AddTicks(9285),
                oldClrType: typeof(DateTime),
                oldType: "TEXT",
                oldDefaultValue: new DateTime(2025, 10, 2, 21, 30, 40, 573, DateTimeKind.Utc).AddTicks(5696));

            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedAt",
                table: "Games",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(2025, 10, 2, 21, 38, 24, 994, DateTimeKind.Utc).AddTicks(7145),
                oldClrType: typeof(DateTime),
                oldType: "TEXT",
                oldDefaultValue: new DateTime(2025, 10, 2, 21, 30, 40, 573, DateTimeKind.Utc).AddTicks(3620));

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "Games",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(2025, 10, 2, 21, 38, 24, 994, DateTimeKind.Utc).AddTicks(6991),
                oldClrType: typeof(DateTime),
                oldType: "TEXT",
                oldDefaultValue: new DateTime(2025, 10, 2, 21, 30, 40, 573, DateTimeKind.Utc).AddTicks(3463));

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "Answers",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(2025, 10, 2, 21, 38, 24, 996, DateTimeKind.Utc).AddTicks(3748),
                oldClrType: typeof(DateTime),
                oldType: "TEXT",
                oldDefaultValue: new DateTime(2025, 10, 2, 21, 30, 40, 574, DateTimeKind.Utc).AddTicks(9980));

            migrationBuilder.AddColumn<string>(
                name: "NormalizedValue",
                table: "Answers",
                type: "TEXT",
                maxLength: 100,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "NormalizedWord",
                table: "WordHiddens");

            migrationBuilder.DropColumn(
                name: "NormalizedValue",
                table: "Answers");

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
                oldDefaultValue: new DateTime(2025, 10, 2, 21, 38, 24, 996, DateTimeKind.Utc).AddTicks(6308));

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "Votes",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(2025, 10, 2, 21, 30, 40, 575, DateTimeKind.Utc).AddTicks(438),
                oldClrType: typeof(DateTime),
                oldType: "TEXT",
                oldDefaultValue: new DateTime(2025, 10, 2, 21, 38, 24, 996, DateTimeKind.Utc).AddTicks(4224));

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
                oldDefaultValue: new DateTime(2025, 10, 2, 21, 38, 24, 996, DateTimeKind.Utc).AddTicks(2524));

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "Questions",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(2025, 10, 2, 21, 30, 40, 575, DateTimeKind.Utc).AddTicks(1360),
                oldClrType: typeof(DateTime),
                oldType: "TEXT",
                oldDefaultValue: new DateTime(2025, 10, 2, 21, 38, 24, 996, DateTimeKind.Utc).AddTicks(5128));

            migrationBuilder.AlterColumn<DateTime>(
                name: "JoinedAt",
                table: "Players",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(2025, 10, 2, 21, 30, 40, 573, DateTimeKind.Utc).AddTicks(5696),
                oldClrType: typeof(DateTime),
                oldType: "TEXT",
                oldDefaultValue: new DateTime(2025, 10, 2, 21, 38, 24, 994, DateTimeKind.Utc).AddTicks(9285));

            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedAt",
                table: "Games",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(2025, 10, 2, 21, 30, 40, 573, DateTimeKind.Utc).AddTicks(3620),
                oldClrType: typeof(DateTime),
                oldType: "TEXT",
                oldDefaultValue: new DateTime(2025, 10, 2, 21, 38, 24, 994, DateTimeKind.Utc).AddTicks(7145));

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "Games",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(2025, 10, 2, 21, 30, 40, 573, DateTimeKind.Utc).AddTicks(3463),
                oldClrType: typeof(DateTime),
                oldType: "TEXT",
                oldDefaultValue: new DateTime(2025, 10, 2, 21, 38, 24, 994, DateTimeKind.Utc).AddTicks(6991));

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "Answers",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(2025, 10, 2, 21, 30, 40, 574, DateTimeKind.Utc).AddTicks(9980),
                oldClrType: typeof(DateTime),
                oldType: "TEXT",
                oldDefaultValue: new DateTime(2025, 10, 2, 21, 38, 24, 996, DateTimeKind.Utc).AddTicks(3748));
        }
    }
}
