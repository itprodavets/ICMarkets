using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace ICMarkets.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "blockchain_data",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Network = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    Chain = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    Height = table.Column<long>(type: "bigint", nullable: false),
                    Hash = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    BlockTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    LatestUrl = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    PreviousHash = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    PreviousUrl = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    PeerCount = table.Column<int>(type: "integer", nullable: false),
                    UnconfirmedCount = table.Column<int>(type: "integer", nullable: false),
                    HighFeePerKb = table.Column<long>(type: "bigint", nullable: true),
                    MediumFeePerKb = table.Column<long>(type: "bigint", nullable: true),
                    LowFeePerKb = table.Column<long>(type: "bigint", nullable: true),
                    HighGasPrice = table.Column<long>(type: "bigint", nullable: true),
                    MediumGasPrice = table.Column<long>(type: "bigint", nullable: true),
                    LowGasPrice = table.Column<long>(type: "bigint", nullable: true),
                    HighPriorityFee = table.Column<long>(type: "bigint", nullable: true),
                    MediumPriorityFee = table.Column<long>(type: "bigint", nullable: true),
                    LowPriorityFee = table.Column<long>(type: "bigint", nullable: true),
                    BaseFee = table.Column<long>(type: "bigint", nullable: true),
                    LastForkHeight = table.Column<long>(type: "bigint", nullable: false),
                    LastForkHash = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_blockchain_data", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_blockchain_data_Network_Chain_CreatedAt",
                table: "blockchain_data",
                columns: new[] { "Network", "Chain", "CreatedAt" },
                descending: new[] { false, false, true });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "blockchain_data");
        }
    }
}
