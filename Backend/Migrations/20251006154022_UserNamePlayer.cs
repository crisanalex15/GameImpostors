using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Backend.Migrations
{
    /// <inheritdoc />
    public partial class UserNamePlayer : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "WordHiddens",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(2025, 10, 6, 15, 40, 21, 957, DateTimeKind.Utc).AddTicks(323),
                oldClrType: typeof(DateTime),
                oldType: "TEXT",
                oldDefaultValue: new DateTime(2025, 9, 24, 22, 20, 49, 156, DateTimeKind.Utc).AddTicks(2136));

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "Votes",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(2025, 10, 6, 15, 40, 21, 956, DateTimeKind.Utc).AddTicks(8256),
                oldClrType: typeof(DateTime),
                oldType: "TEXT",
                oldDefaultValue: new DateTime(2025, 9, 24, 22, 20, 49, 155, DateTimeKind.Utc).AddTicks(9935));

            migrationBuilder.AlterColumn<DateTime>(
                name: "StartedAt",
                table: "Rounds",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(2025, 10, 6, 15, 40, 21, 956, DateTimeKind.Utc).AddTicks(6526),
                oldClrType: typeof(DateTime),
                oldType: "TEXT",
                oldDefaultValue: new DateTime(2025, 9, 24, 22, 20, 49, 155, DateTimeKind.Utc).AddTicks(8156));

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "Questions",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(2025, 10, 6, 15, 40, 21, 956, DateTimeKind.Utc).AddTicks(9177),
                oldClrType: typeof(DateTime),
                oldType: "TEXT",
                oldDefaultValue: new DateTime(2025, 9, 24, 22, 20, 49, 156, DateTimeKind.Utc).AddTicks(852));

            migrationBuilder.AlterColumn<DateTime>(
                name: "JoinedAt",
                table: "Players",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(2025, 10, 6, 15, 40, 21, 955, DateTimeKind.Utc).AddTicks(2471),
                oldClrType: typeof(DateTime),
                oldType: "TEXT",
                oldDefaultValue: new DateTime(2025, 9, 24, 22, 20, 49, 154, DateTimeKind.Utc).AddTicks(3799));

            migrationBuilder.AddColumn<string>(
                name: "UserName",
                table: "Players",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedAt",
                table: "Games",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(2025, 10, 6, 15, 40, 21, 955, DateTimeKind.Utc).AddTicks(35),
                oldClrType: typeof(DateTime),
                oldType: "TEXT",
                oldDefaultValue: new DateTime(2025, 9, 24, 22, 20, 49, 154, DateTimeKind.Utc).AddTicks(1328));

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "Games",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(2025, 10, 6, 15, 40, 21, 954, DateTimeKind.Utc).AddTicks(9906),
                oldClrType: typeof(DateTime),
                oldType: "TEXT",
                oldDefaultValue: new DateTime(2025, 9, 24, 22, 20, 49, 154, DateTimeKind.Utc).AddTicks(1167));

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "Answers",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(2025, 10, 6, 15, 40, 21, 956, DateTimeKind.Utc).AddTicks(7820),
                oldClrType: typeof(DateTime),
                oldType: "TEXT",
                oldDefaultValue: new DateTime(2025, 9, 24, 22, 20, 49, 155, DateTimeKind.Utc).AddTicks(9438));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "UserName",
                table: "Players");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "WordHiddens",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(2025, 9, 24, 22, 20, 49, 156, DateTimeKind.Utc).AddTicks(2136),
                oldClrType: typeof(DateTime),
                oldType: "TEXT",
                oldDefaultValue: new DateTime(2025, 10, 6, 15, 40, 21, 957, DateTimeKind.Utc).AddTicks(323));

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "Votes",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(2025, 9, 24, 22, 20, 49, 155, DateTimeKind.Utc).AddTicks(9935),
                oldClrType: typeof(DateTime),
                oldType: "TEXT",
                oldDefaultValue: new DateTime(2025, 10, 6, 15, 40, 21, 956, DateTimeKind.Utc).AddTicks(8256));

            migrationBuilder.AlterColumn<DateTime>(
                name: "StartedAt",
                table: "Rounds",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(2025, 9, 24, 22, 20, 49, 155, DateTimeKind.Utc).AddTicks(8156),
                oldClrType: typeof(DateTime),
                oldType: "TEXT",
                oldDefaultValue: new DateTime(2025, 10, 6, 15, 40, 21, 956, DateTimeKind.Utc).AddTicks(6526));

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "Questions",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(2025, 9, 24, 22, 20, 49, 156, DateTimeKind.Utc).AddTicks(852),
                oldClrType: typeof(DateTime),
                oldType: "TEXT",
                oldDefaultValue: new DateTime(2025, 10, 6, 15, 40, 21, 956, DateTimeKind.Utc).AddTicks(9177));

            migrationBuilder.AlterColumn<DateTime>(
                name: "JoinedAt",
                table: "Players",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(2025, 9, 24, 22, 20, 49, 154, DateTimeKind.Utc).AddTicks(3799),
                oldClrType: typeof(DateTime),
                oldType: "TEXT",
                oldDefaultValue: new DateTime(2025, 10, 6, 15, 40, 21, 955, DateTimeKind.Utc).AddTicks(2471));

            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedAt",
                table: "Games",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(2025, 9, 24, 22, 20, 49, 154, DateTimeKind.Utc).AddTicks(1328),
                oldClrType: typeof(DateTime),
                oldType: "TEXT",
                oldDefaultValue: new DateTime(2025, 10, 6, 15, 40, 21, 955, DateTimeKind.Utc).AddTicks(35));

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "Games",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(2025, 9, 24, 22, 20, 49, 154, DateTimeKind.Utc).AddTicks(1167),
                oldClrType: typeof(DateTime),
                oldType: "TEXT",
                oldDefaultValue: new DateTime(2025, 10, 6, 15, 40, 21, 954, DateTimeKind.Utc).AddTicks(9906));

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "Answers",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(2025, 9, 24, 22, 20, 49, 155, DateTimeKind.Utc).AddTicks(9438),
                oldClrType: typeof(DateTime),
                oldType: "TEXT",
                oldDefaultValue: new DateTime(2025, 10, 6, 15, 40, 21, 956, DateTimeKind.Utc).AddTicks(7820));
        }
    }
}
