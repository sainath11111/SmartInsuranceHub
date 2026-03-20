using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace SmartInsuranceHub.Migrations
{
    /// <inheritdoc />
    public partial class AddChatSystem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Agents_Companies_company_id",
                table: "Agents");

            migrationBuilder.DropForeignKey(
                name: "FK_InsurancePlans_Agents_agent_id",
                table: "InsurancePlans");

            migrationBuilder.DropForeignKey(
                name: "FK_InsurancePlans_Companies_company_id",
                table: "InsurancePlans");

            migrationBuilder.DropForeignKey(
                name: "FK_InsuranceTypes_Companies_company_id",
                table: "InsuranceTypes");

            migrationBuilder.DropForeignKey(
                name: "FK_Payments_Agents_received_by_agent",
                table: "Payments");

            migrationBuilder.DropForeignKey(
                name: "FK_Payments_Customers_customer_id",
                table: "Payments");

            migrationBuilder.DropForeignKey(
                name: "FK_Payments_Policies_policy_id",
                table: "Payments");

            migrationBuilder.DropForeignKey(
                name: "FK_Policies_Agents_agent_id",
                table: "Policies");

            migrationBuilder.DropForeignKey(
                name: "FK_Policies_Customers_customer_id",
                table: "Policies");

            migrationBuilder.DropForeignKey(
                name: "FK_Policies_InsurancePlans_plan_id_company_id",
                table: "Policies");

            migrationBuilder.DropForeignKey(
                name: "FK_Queries_Customers_customer_id",
                table: "Queries");

            migrationBuilder.DropForeignKey(
                name: "FK_Reviews_Customers_customer_id",
                table: "Reviews");

            migrationBuilder.DropForeignKey(
                name: "FK_Reviews_InsurancePlans_plan_id_company_id",
                table: "Reviews");

            migrationBuilder.DropPrimaryKey(
                name: "PK_InsuranceTypes",
                table: "InsuranceTypes");

            migrationBuilder.DropIndex(
                name: "IX_InsuranceTypes_company_id",
                table: "InsuranceTypes");

            migrationBuilder.DropColumn(
                name: "company_id",
                table: "InsuranceTypes");

            migrationBuilder.DropColumn(
                name: "created_by",
                table: "InsuranceTypes");

            migrationBuilder.RenameColumn(
                name: "rating",
                table: "Reviews",
                newName: "Rating");

            migrationBuilder.RenameColumn(
                name: "comment",
                table: "Reviews",
                newName: "Comment");

            migrationBuilder.RenameColumn(
                name: "plan_id",
                table: "Reviews",
                newName: "PlanId");

            migrationBuilder.RenameColumn(
                name: "customer_id",
                table: "Reviews",
                newName: "CustomerId");

            migrationBuilder.RenameColumn(
                name: "created_at",
                table: "Reviews",
                newName: "CreatedAt");

            migrationBuilder.RenameColumn(
                name: "company_id",
                table: "Reviews",
                newName: "CompanyId");

            migrationBuilder.RenameColumn(
                name: "review_id",
                table: "Reviews",
                newName: "ReviewId");

            migrationBuilder.RenameIndex(
                name: "IX_Reviews_plan_id_company_id",
                table: "Reviews",
                newName: "IX_Reviews_PlanId_CompanyId");

            migrationBuilder.RenameIndex(
                name: "IX_Reviews_customer_id",
                table: "Reviews",
                newName: "IX_Reviews_CustomerId");

            migrationBuilder.RenameColumn(
                name: "subject",
                table: "Queries",
                newName: "Subject");

            migrationBuilder.RenameColumn(
                name: "phone",
                table: "Queries",
                newName: "Phone");

            migrationBuilder.RenameColumn(
                name: "name",
                table: "Queries",
                newName: "Name");

            migrationBuilder.RenameColumn(
                name: "message",
                table: "Queries",
                newName: "Message");

            migrationBuilder.RenameColumn(
                name: "email",
                table: "Queries",
                newName: "Email");

            migrationBuilder.RenameColumn(
                name: "send_date",
                table: "Queries",
                newName: "SendDate");

            migrationBuilder.RenameColumn(
                name: "customer_id",
                table: "Queries",
                newName: "CustomerId");

            migrationBuilder.RenameColumn(
                name: "query_id",
                table: "Queries",
                newName: "QueryId");

            migrationBuilder.RenameIndex(
                name: "IX_Queries_customer_id",
                table: "Queries",
                newName: "IX_Queries_CustomerId");

            migrationBuilder.RenameColumn(
                name: "start_date",
                table: "Policies",
                newName: "StartDate");

            migrationBuilder.RenameColumn(
                name: "premium_amount",
                table: "Policies",
                newName: "PremiumAmount");

            migrationBuilder.RenameColumn(
                name: "policy_status",
                table: "Policies",
                newName: "PolicyStatus");

            migrationBuilder.RenameColumn(
                name: "policy_no",
                table: "Policies",
                newName: "PolicyNo");

            migrationBuilder.RenameColumn(
                name: "plan_id",
                table: "Policies",
                newName: "PlanId");

            migrationBuilder.RenameColumn(
                name: "end_date",
                table: "Policies",
                newName: "EndDate");

            migrationBuilder.RenameColumn(
                name: "customer_id",
                table: "Policies",
                newName: "CustomerId");

            migrationBuilder.RenameColumn(
                name: "created_by",
                table: "Policies",
                newName: "CreatedBy");

            migrationBuilder.RenameColumn(
                name: "company_id",
                table: "Policies",
                newName: "CompanyId");

            migrationBuilder.RenameColumn(
                name: "agent_id",
                table: "Policies",
                newName: "AgentId");

            migrationBuilder.RenameColumn(
                name: "policy_id",
                table: "Policies",
                newName: "PolicyId");

            migrationBuilder.RenameIndex(
                name: "IX_Policies_plan_id_company_id",
                table: "Policies",
                newName: "IX_Policies_PlanId_CompanyId");

            migrationBuilder.RenameIndex(
                name: "IX_Policies_customer_id",
                table: "Policies",
                newName: "IX_Policies_CustomerId");

            migrationBuilder.RenameIndex(
                name: "IX_Policies_agent_id",
                table: "Policies",
                newName: "IX_Policies_AgentId");

            migrationBuilder.RenameColumn(
                name: "method",
                table: "Payments",
                newName: "Method");

            migrationBuilder.RenameColumn(
                name: "amount",
                table: "Payments",
                newName: "Amount");

            migrationBuilder.RenameColumn(
                name: "received_by_agent",
                table: "Payments",
                newName: "ReceivedByAgent");

            migrationBuilder.RenameColumn(
                name: "policy_id",
                table: "Payments",
                newName: "PolicyId");

            migrationBuilder.RenameColumn(
                name: "payment_status",
                table: "Payments",
                newName: "PaymentStatus");

            migrationBuilder.RenameColumn(
                name: "payment_date",
                table: "Payments",
                newName: "PaymentDate");

            migrationBuilder.RenameColumn(
                name: "customer_id",
                table: "Payments",
                newName: "CustomerId");

            migrationBuilder.RenameColumn(
                name: "payment_id",
                table: "Payments",
                newName: "PaymentId");

            migrationBuilder.RenameIndex(
                name: "IX_Payments_received_by_agent",
                table: "Payments",
                newName: "IX_Payments_ReceivedByAgent");

            migrationBuilder.RenameIndex(
                name: "IX_Payments_policy_id",
                table: "Payments",
                newName: "IX_Payments_PolicyId");

            migrationBuilder.RenameIndex(
                name: "IX_Payments_customer_id",
                table: "Payments",
                newName: "IX_Payments_CustomerId");

            migrationBuilder.RenameColumn(
                name: "status",
                table: "InsuranceTypes",
                newName: "Status");

            migrationBuilder.RenameColumn(
                name: "description",
                table: "InsuranceTypes",
                newName: "Description");

            migrationBuilder.RenameColumn(
                name: "type_name",
                table: "InsuranceTypes",
                newName: "TypeName");

            migrationBuilder.RenameColumn(
                name: "type_id",
                table: "InsuranceTypes",
                newName: "TypeId");

            migrationBuilder.RenameColumn(
                name: "created_date",
                table: "InsuranceTypes",
                newName: "CreatedAt");

            migrationBuilder.RenameColumn(
                name: "status",
                table: "InsurancePlans",
                newName: "Status");

            migrationBuilder.RenameColumn(
                name: "description",
                table: "InsurancePlans",
                newName: "Description");

            migrationBuilder.RenameColumn(
                name: "type_id",
                table: "InsurancePlans",
                newName: "TypeId");

            migrationBuilder.RenameColumn(
                name: "premium_amount",
                table: "InsurancePlans",
                newName: "PremiumAmount");

            migrationBuilder.RenameColumn(
                name: "plan_name",
                table: "InsurancePlans",
                newName: "PlanName");

            migrationBuilder.RenameColumn(
                name: "duration_months",
                table: "InsurancePlans",
                newName: "DurationMonths");

            migrationBuilder.RenameColumn(
                name: "created_date",
                table: "InsurancePlans",
                newName: "CreatedDate");

            migrationBuilder.RenameColumn(
                name: "coverage_amount",
                table: "InsurancePlans",
                newName: "CoverageAmount");

            migrationBuilder.RenameColumn(
                name: "agent_id",
                table: "InsurancePlans",
                newName: "AgentId");

            migrationBuilder.RenameColumn(
                name: "company_id",
                table: "InsurancePlans",
                newName: "CompanyId");

            migrationBuilder.RenameColumn(
                name: "plan_id",
                table: "InsurancePlans",
                newName: "PlanId");

            migrationBuilder.RenameIndex(
                name: "IX_InsurancePlans_company_id",
                table: "InsurancePlans",
                newName: "IX_InsurancePlans_CompanyId");

            migrationBuilder.RenameIndex(
                name: "IX_InsurancePlans_agent_id",
                table: "InsurancePlans",
                newName: "IX_InsurancePlans_AgentId");

            migrationBuilder.RenameColumn(
                name: "status",
                table: "Customers",
                newName: "Status");

            migrationBuilder.RenameColumn(
                name: "phone",
                table: "Customers",
                newName: "Phone");

            migrationBuilder.RenameColumn(
                name: "password",
                table: "Customers",
                newName: "Password");

            migrationBuilder.RenameColumn(
                name: "email",
                table: "Customers",
                newName: "Email");

            migrationBuilder.RenameColumn(
                name: "city",
                table: "Customers",
                newName: "City");

            migrationBuilder.RenameColumn(
                name: "age",
                table: "Customers",
                newName: "Age");

            migrationBuilder.RenameColumn(
                name: "address",
                table: "Customers",
                newName: "Address");

            migrationBuilder.RenameColumn(
                name: "policy_id",
                table: "Customers",
                newName: "PolicyId");

            migrationBuilder.RenameColumn(
                name: "full_name",
                table: "Customers",
                newName: "FullName");

            migrationBuilder.RenameColumn(
                name: "family_info",
                table: "Customers",
                newName: "FamilyInfo");

            migrationBuilder.RenameColumn(
                name: "created_at",
                table: "Customers",
                newName: "CreatedAt");

            migrationBuilder.RenameColumn(
                name: "c_pancard",
                table: "Customers",
                newName: "CPancard");

            migrationBuilder.RenameColumn(
                name: "c_adhar",
                table: "Customers",
                newName: "CAdhar");

            migrationBuilder.RenameColumn(
                name: "customer_id",
                table: "Customers",
                newName: "CustomerId");

            migrationBuilder.RenameColumn(
                name: "status",
                table: "Companies",
                newName: "Status");

            migrationBuilder.RenameColumn(
                name: "password",
                table: "Companies",
                newName: "Password");

            migrationBuilder.RenameColumn(
                name: "email",
                table: "Companies",
                newName: "Email");

            migrationBuilder.RenameColumn(
                name: "address",
                table: "Companies",
                newName: "Address");

            migrationBuilder.RenameColumn(
                name: "license_number",
                table: "Companies",
                newName: "LicenseNumber");

            migrationBuilder.RenameColumn(
                name: "created_at",
                table: "Companies",
                newName: "CreatedAt");

            migrationBuilder.RenameColumn(
                name: "company_name",
                table: "Companies",
                newName: "CompanyName");

            migrationBuilder.RenameColumn(
                name: "c_information",
                table: "Companies",
                newName: "CInformation");

            migrationBuilder.RenameColumn(
                name: "c_agent",
                table: "Companies",
                newName: "CAgent");

            migrationBuilder.RenameColumn(
                name: "company_id",
                table: "Companies",
                newName: "CompanyId");

            migrationBuilder.RenameColumn(
                name: "pincode",
                table: "Agents",
                newName: "Pincode");

            migrationBuilder.RenameColumn(
                name: "phone",
                table: "Agents",
                newName: "Phone");

            migrationBuilder.RenameColumn(
                name: "password",
                table: "Agents",
                newName: "Password");

            migrationBuilder.RenameColumn(
                name: "email",
                table: "Agents",
                newName: "Email");

            migrationBuilder.RenameColumn(
                name: "city",
                table: "Agents",
                newName: "City");

            migrationBuilder.RenameColumn(
                name: "address",
                table: "Agents",
                newName: "Address");

            migrationBuilder.RenameColumn(
                name: "profile_photo",
                table: "Agents",
                newName: "ProfilePhoto");

            migrationBuilder.RenameColumn(
                name: "license_number",
                table: "Agents",
                newName: "LicenseNumber");

            migrationBuilder.RenameColumn(
                name: "full_name",
                table: "Agents",
                newName: "FullName");

            migrationBuilder.RenameColumn(
                name: "experience_years",
                table: "Agents",
                newName: "ExperienceYears");

            migrationBuilder.RenameColumn(
                name: "created_at",
                table: "Agents",
                newName: "CreatedAt");

            migrationBuilder.RenameColumn(
                name: "company_id",
                table: "Agents",
                newName: "CompanyId");

            migrationBuilder.RenameColumn(
                name: "approved_status",
                table: "Agents",
                newName: "ApprovedStatus");

            migrationBuilder.RenameColumn(
                name: "agent_id",
                table: "Agents",
                newName: "AgentId");

            migrationBuilder.RenameIndex(
                name: "IX_Agents_company_id",
                table: "Agents",
                newName: "IX_Agents_CompanyId");

            migrationBuilder.RenameColumn(
                name: "phone",
                table: "Admins",
                newName: "Phone");

            migrationBuilder.RenameColumn(
                name: "password",
                table: "Admins",
                newName: "Password");

            migrationBuilder.RenameColumn(
                name: "email",
                table: "Admins",
                newName: "Email");

            migrationBuilder.RenameColumn(
                name: "last_login",
                table: "Admins",
                newName: "LastLogin");

            migrationBuilder.RenameColumn(
                name: "full_name",
                table: "Admins",
                newName: "FullName");

            migrationBuilder.RenameColumn(
                name: "created_at",
                table: "Admins",
                newName: "CreatedAt");

            migrationBuilder.RenameColumn(
                name: "admin_id",
                table: "Admins",
                newName: "AdminId");

            migrationBuilder.AlterColumn<string>(
                name: "Comment",
                table: "Reviews",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "Subject",
                table: "Queries",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(200)",
                oldMaxLength: 200);

            migrationBuilder.AlterColumn<string>(
                name: "Phone",
                table: "Queries",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(20)",
                oldMaxLength: 20);

            migrationBuilder.AlterColumn<string>(
                name: "Message",
                table: "Queries",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "CreatedBy",
                table: "Policies",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<string>(
                name: "Method",
                table: "Payments",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(50)",
                oldMaxLength: 50);

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "InsuranceTypes",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<int>(
                name: "TypeId",
                table: "InsuranceTypes",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer")
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn)
                .OldAnnotation("Relational:ColumnOrder", 1);

            migrationBuilder.AddColumn<string>(
                name: "Icon",
                table: "InsuranceTypes",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "InsurancePlans",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "Phone",
                table: "Customers",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(20)",
                oldMaxLength: 20);

            migrationBuilder.AlterColumn<string>(
                name: "City",
                table: "Customers",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<string>(
                name: "Address",
                table: "Customers",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "FamilyInfo",
                table: "Customers",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "CPancard",
                table: "Customers",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(20)",
                oldMaxLength: 20);

            migrationBuilder.AlterColumn<string>(
                name: "CAdhar",
                table: "Customers",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(20)",
                oldMaxLength: 20);

            migrationBuilder.AlterColumn<string>(
                name: "Address",
                table: "Companies",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "LicenseNumber",
                table: "Companies",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(50)",
                oldMaxLength: 50);

            migrationBuilder.AlterColumn<string>(
                name: "CInformation",
                table: "Companies",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "CAgent",
                table: "Companies",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<string>(
                name: "Pincode",
                table: "Agents",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(20)",
                oldMaxLength: 20);

            migrationBuilder.AlterColumn<string>(
                name: "Phone",
                table: "Agents",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(20)",
                oldMaxLength: 20);

            migrationBuilder.AlterColumn<string>(
                name: "City",
                table: "Agents",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<string>(
                name: "Address",
                table: "Agents",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "ProfilePhoto",
                table: "Agents",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "LicenseNumber",
                table: "Agents",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(50)",
                oldMaxLength: 50);

            migrationBuilder.AddPrimaryKey(
                name: "PK_InsuranceTypes",
                table: "InsuranceTypes",
                column: "TypeId");

            migrationBuilder.CreateTable(
                name: "ChatMessages",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CompanyId = table.Column<int>(type: "integer", nullable: false),
                    AgentId = table.Column<int>(type: "integer", nullable: false),
                    SenderType = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    MessageText = table.Column<string>(type: "text", nullable: false),
                    SentAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChatMessages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ChatMessages_Agents_AgentId",
                        column: x => x.AgentId,
                        principalTable: "Agents",
                        principalColumn: "AgentId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ChatMessages_Companies_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "Companies",
                        principalColumn: "CompanyId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ChatMessages_AgentId",
                table: "ChatMessages",
                column: "AgentId");

            migrationBuilder.CreateIndex(
                name: "IX_ChatMessages_CompanyId",
                table: "ChatMessages",
                column: "CompanyId");

            migrationBuilder.AddForeignKey(
                name: "FK_Agents_Companies_CompanyId",
                table: "Agents",
                column: "CompanyId",
                principalTable: "Companies",
                principalColumn: "CompanyId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_InsurancePlans_Agents_AgentId",
                table: "InsurancePlans",
                column: "AgentId",
                principalTable: "Agents",
                principalColumn: "AgentId");

            migrationBuilder.AddForeignKey(
                name: "FK_InsurancePlans_Companies_CompanyId",
                table: "InsurancePlans",
                column: "CompanyId",
                principalTable: "Companies",
                principalColumn: "CompanyId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Payments_Agents_ReceivedByAgent",
                table: "Payments",
                column: "ReceivedByAgent",
                principalTable: "Agents",
                principalColumn: "AgentId");

            migrationBuilder.AddForeignKey(
                name: "FK_Payments_Customers_CustomerId",
                table: "Payments",
                column: "CustomerId",
                principalTable: "Customers",
                principalColumn: "CustomerId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Payments_Policies_PolicyId",
                table: "Payments",
                column: "PolicyId",
                principalTable: "Policies",
                principalColumn: "PolicyId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Policies_Agents_AgentId",
                table: "Policies",
                column: "AgentId",
                principalTable: "Agents",
                principalColumn: "AgentId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Policies_Customers_CustomerId",
                table: "Policies",
                column: "CustomerId",
                principalTable: "Customers",
                principalColumn: "CustomerId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Policies_InsurancePlans_PlanId_CompanyId",
                table: "Policies",
                columns: new[] { "PlanId", "CompanyId" },
                principalTable: "InsurancePlans",
                principalColumns: new[] { "PlanId", "CompanyId" },
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Queries_Customers_CustomerId",
                table: "Queries",
                column: "CustomerId",
                principalTable: "Customers",
                principalColumn: "CustomerId");

            migrationBuilder.AddForeignKey(
                name: "FK_Reviews_Customers_CustomerId",
                table: "Reviews",
                column: "CustomerId",
                principalTable: "Customers",
                principalColumn: "CustomerId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Reviews_InsurancePlans_PlanId_CompanyId",
                table: "Reviews",
                columns: new[] { "PlanId", "CompanyId" },
                principalTable: "InsurancePlans",
                principalColumns: new[] { "PlanId", "CompanyId" },
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Agents_Companies_CompanyId",
                table: "Agents");

            migrationBuilder.DropForeignKey(
                name: "FK_InsurancePlans_Agents_AgentId",
                table: "InsurancePlans");

            migrationBuilder.DropForeignKey(
                name: "FK_InsurancePlans_Companies_CompanyId",
                table: "InsurancePlans");

            migrationBuilder.DropForeignKey(
                name: "FK_Payments_Agents_ReceivedByAgent",
                table: "Payments");

            migrationBuilder.DropForeignKey(
                name: "FK_Payments_Customers_CustomerId",
                table: "Payments");

            migrationBuilder.DropForeignKey(
                name: "FK_Payments_Policies_PolicyId",
                table: "Payments");

            migrationBuilder.DropForeignKey(
                name: "FK_Policies_Agents_AgentId",
                table: "Policies");

            migrationBuilder.DropForeignKey(
                name: "FK_Policies_Customers_CustomerId",
                table: "Policies");

            migrationBuilder.DropForeignKey(
                name: "FK_Policies_InsurancePlans_PlanId_CompanyId",
                table: "Policies");

            migrationBuilder.DropForeignKey(
                name: "FK_Queries_Customers_CustomerId",
                table: "Queries");

            migrationBuilder.DropForeignKey(
                name: "FK_Reviews_Customers_CustomerId",
                table: "Reviews");

            migrationBuilder.DropForeignKey(
                name: "FK_Reviews_InsurancePlans_PlanId_CompanyId",
                table: "Reviews");

            migrationBuilder.DropTable(
                name: "ChatMessages");

            migrationBuilder.DropPrimaryKey(
                name: "PK_InsuranceTypes",
                table: "InsuranceTypes");

            migrationBuilder.DropColumn(
                name: "Icon",
                table: "InsuranceTypes");

            migrationBuilder.RenameColumn(
                name: "Rating",
                table: "Reviews",
                newName: "rating");

            migrationBuilder.RenameColumn(
                name: "Comment",
                table: "Reviews",
                newName: "comment");

            migrationBuilder.RenameColumn(
                name: "PlanId",
                table: "Reviews",
                newName: "plan_id");

            migrationBuilder.RenameColumn(
                name: "CustomerId",
                table: "Reviews",
                newName: "customer_id");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "Reviews",
                newName: "created_at");

            migrationBuilder.RenameColumn(
                name: "CompanyId",
                table: "Reviews",
                newName: "company_id");

            migrationBuilder.RenameColumn(
                name: "ReviewId",
                table: "Reviews",
                newName: "review_id");

            migrationBuilder.RenameIndex(
                name: "IX_Reviews_PlanId_CompanyId",
                table: "Reviews",
                newName: "IX_Reviews_plan_id_company_id");

            migrationBuilder.RenameIndex(
                name: "IX_Reviews_CustomerId",
                table: "Reviews",
                newName: "IX_Reviews_customer_id");

            migrationBuilder.RenameColumn(
                name: "Subject",
                table: "Queries",
                newName: "subject");

            migrationBuilder.RenameColumn(
                name: "Phone",
                table: "Queries",
                newName: "phone");

            migrationBuilder.RenameColumn(
                name: "Name",
                table: "Queries",
                newName: "name");

            migrationBuilder.RenameColumn(
                name: "Message",
                table: "Queries",
                newName: "message");

            migrationBuilder.RenameColumn(
                name: "Email",
                table: "Queries",
                newName: "email");

            migrationBuilder.RenameColumn(
                name: "SendDate",
                table: "Queries",
                newName: "send_date");

            migrationBuilder.RenameColumn(
                name: "CustomerId",
                table: "Queries",
                newName: "customer_id");

            migrationBuilder.RenameColumn(
                name: "QueryId",
                table: "Queries",
                newName: "query_id");

            migrationBuilder.RenameIndex(
                name: "IX_Queries_CustomerId",
                table: "Queries",
                newName: "IX_Queries_customer_id");

            migrationBuilder.RenameColumn(
                name: "StartDate",
                table: "Policies",
                newName: "start_date");

            migrationBuilder.RenameColumn(
                name: "PremiumAmount",
                table: "Policies",
                newName: "premium_amount");

            migrationBuilder.RenameColumn(
                name: "PolicyStatus",
                table: "Policies",
                newName: "policy_status");

            migrationBuilder.RenameColumn(
                name: "PolicyNo",
                table: "Policies",
                newName: "policy_no");

            migrationBuilder.RenameColumn(
                name: "PlanId",
                table: "Policies",
                newName: "plan_id");

            migrationBuilder.RenameColumn(
                name: "EndDate",
                table: "Policies",
                newName: "end_date");

            migrationBuilder.RenameColumn(
                name: "CustomerId",
                table: "Policies",
                newName: "customer_id");

            migrationBuilder.RenameColumn(
                name: "CreatedBy",
                table: "Policies",
                newName: "created_by");

            migrationBuilder.RenameColumn(
                name: "CompanyId",
                table: "Policies",
                newName: "company_id");

            migrationBuilder.RenameColumn(
                name: "AgentId",
                table: "Policies",
                newName: "agent_id");

            migrationBuilder.RenameColumn(
                name: "PolicyId",
                table: "Policies",
                newName: "policy_id");

            migrationBuilder.RenameIndex(
                name: "IX_Policies_PlanId_CompanyId",
                table: "Policies",
                newName: "IX_Policies_plan_id_company_id");

            migrationBuilder.RenameIndex(
                name: "IX_Policies_CustomerId",
                table: "Policies",
                newName: "IX_Policies_customer_id");

            migrationBuilder.RenameIndex(
                name: "IX_Policies_AgentId",
                table: "Policies",
                newName: "IX_Policies_agent_id");

            migrationBuilder.RenameColumn(
                name: "Method",
                table: "Payments",
                newName: "method");

            migrationBuilder.RenameColumn(
                name: "Amount",
                table: "Payments",
                newName: "amount");

            migrationBuilder.RenameColumn(
                name: "ReceivedByAgent",
                table: "Payments",
                newName: "received_by_agent");

            migrationBuilder.RenameColumn(
                name: "PolicyId",
                table: "Payments",
                newName: "policy_id");

            migrationBuilder.RenameColumn(
                name: "PaymentStatus",
                table: "Payments",
                newName: "payment_status");

            migrationBuilder.RenameColumn(
                name: "PaymentDate",
                table: "Payments",
                newName: "payment_date");

            migrationBuilder.RenameColumn(
                name: "CustomerId",
                table: "Payments",
                newName: "customer_id");

            migrationBuilder.RenameColumn(
                name: "PaymentId",
                table: "Payments",
                newName: "payment_id");

            migrationBuilder.RenameIndex(
                name: "IX_Payments_ReceivedByAgent",
                table: "Payments",
                newName: "IX_Payments_received_by_agent");

            migrationBuilder.RenameIndex(
                name: "IX_Payments_PolicyId",
                table: "Payments",
                newName: "IX_Payments_policy_id");

            migrationBuilder.RenameIndex(
                name: "IX_Payments_CustomerId",
                table: "Payments",
                newName: "IX_Payments_customer_id");

            migrationBuilder.RenameColumn(
                name: "Status",
                table: "InsuranceTypes",
                newName: "status");

            migrationBuilder.RenameColumn(
                name: "Description",
                table: "InsuranceTypes",
                newName: "description");

            migrationBuilder.RenameColumn(
                name: "TypeName",
                table: "InsuranceTypes",
                newName: "type_name");

            migrationBuilder.RenameColumn(
                name: "TypeId",
                table: "InsuranceTypes",
                newName: "type_id");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "InsuranceTypes",
                newName: "created_date");

            migrationBuilder.RenameColumn(
                name: "Status",
                table: "InsurancePlans",
                newName: "status");

            migrationBuilder.RenameColumn(
                name: "Description",
                table: "InsurancePlans",
                newName: "description");

            migrationBuilder.RenameColumn(
                name: "TypeId",
                table: "InsurancePlans",
                newName: "type_id");

            migrationBuilder.RenameColumn(
                name: "PremiumAmount",
                table: "InsurancePlans",
                newName: "premium_amount");

            migrationBuilder.RenameColumn(
                name: "PlanName",
                table: "InsurancePlans",
                newName: "plan_name");

            migrationBuilder.RenameColumn(
                name: "DurationMonths",
                table: "InsurancePlans",
                newName: "duration_months");

            migrationBuilder.RenameColumn(
                name: "CreatedDate",
                table: "InsurancePlans",
                newName: "created_date");

            migrationBuilder.RenameColumn(
                name: "CoverageAmount",
                table: "InsurancePlans",
                newName: "coverage_amount");

            migrationBuilder.RenameColumn(
                name: "AgentId",
                table: "InsurancePlans",
                newName: "agent_id");

            migrationBuilder.RenameColumn(
                name: "CompanyId",
                table: "InsurancePlans",
                newName: "company_id");

            migrationBuilder.RenameColumn(
                name: "PlanId",
                table: "InsurancePlans",
                newName: "plan_id");

            migrationBuilder.RenameIndex(
                name: "IX_InsurancePlans_CompanyId",
                table: "InsurancePlans",
                newName: "IX_InsurancePlans_company_id");

            migrationBuilder.RenameIndex(
                name: "IX_InsurancePlans_AgentId",
                table: "InsurancePlans",
                newName: "IX_InsurancePlans_agent_id");

            migrationBuilder.RenameColumn(
                name: "Status",
                table: "Customers",
                newName: "status");

            migrationBuilder.RenameColumn(
                name: "Phone",
                table: "Customers",
                newName: "phone");

            migrationBuilder.RenameColumn(
                name: "Password",
                table: "Customers",
                newName: "password");

            migrationBuilder.RenameColumn(
                name: "Email",
                table: "Customers",
                newName: "email");

            migrationBuilder.RenameColumn(
                name: "City",
                table: "Customers",
                newName: "city");

            migrationBuilder.RenameColumn(
                name: "Age",
                table: "Customers",
                newName: "age");

            migrationBuilder.RenameColumn(
                name: "Address",
                table: "Customers",
                newName: "address");

            migrationBuilder.RenameColumn(
                name: "PolicyId",
                table: "Customers",
                newName: "policy_id");

            migrationBuilder.RenameColumn(
                name: "FullName",
                table: "Customers",
                newName: "full_name");

            migrationBuilder.RenameColumn(
                name: "FamilyInfo",
                table: "Customers",
                newName: "family_info");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "Customers",
                newName: "created_at");

            migrationBuilder.RenameColumn(
                name: "CPancard",
                table: "Customers",
                newName: "c_pancard");

            migrationBuilder.RenameColumn(
                name: "CAdhar",
                table: "Customers",
                newName: "c_adhar");

            migrationBuilder.RenameColumn(
                name: "CustomerId",
                table: "Customers",
                newName: "customer_id");

            migrationBuilder.RenameColumn(
                name: "Status",
                table: "Companies",
                newName: "status");

            migrationBuilder.RenameColumn(
                name: "Password",
                table: "Companies",
                newName: "password");

            migrationBuilder.RenameColumn(
                name: "Email",
                table: "Companies",
                newName: "email");

            migrationBuilder.RenameColumn(
                name: "Address",
                table: "Companies",
                newName: "address");

            migrationBuilder.RenameColumn(
                name: "LicenseNumber",
                table: "Companies",
                newName: "license_number");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "Companies",
                newName: "created_at");

            migrationBuilder.RenameColumn(
                name: "CompanyName",
                table: "Companies",
                newName: "company_name");

            migrationBuilder.RenameColumn(
                name: "CInformation",
                table: "Companies",
                newName: "c_information");

            migrationBuilder.RenameColumn(
                name: "CAgent",
                table: "Companies",
                newName: "c_agent");

            migrationBuilder.RenameColumn(
                name: "CompanyId",
                table: "Companies",
                newName: "company_id");

            migrationBuilder.RenameColumn(
                name: "Pincode",
                table: "Agents",
                newName: "pincode");

            migrationBuilder.RenameColumn(
                name: "Phone",
                table: "Agents",
                newName: "phone");

            migrationBuilder.RenameColumn(
                name: "Password",
                table: "Agents",
                newName: "password");

            migrationBuilder.RenameColumn(
                name: "Email",
                table: "Agents",
                newName: "email");

            migrationBuilder.RenameColumn(
                name: "City",
                table: "Agents",
                newName: "city");

            migrationBuilder.RenameColumn(
                name: "Address",
                table: "Agents",
                newName: "address");

            migrationBuilder.RenameColumn(
                name: "ProfilePhoto",
                table: "Agents",
                newName: "profile_photo");

            migrationBuilder.RenameColumn(
                name: "LicenseNumber",
                table: "Agents",
                newName: "license_number");

            migrationBuilder.RenameColumn(
                name: "FullName",
                table: "Agents",
                newName: "full_name");

            migrationBuilder.RenameColumn(
                name: "ExperienceYears",
                table: "Agents",
                newName: "experience_years");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "Agents",
                newName: "created_at");

            migrationBuilder.RenameColumn(
                name: "CompanyId",
                table: "Agents",
                newName: "company_id");

            migrationBuilder.RenameColumn(
                name: "ApprovedStatus",
                table: "Agents",
                newName: "approved_status");

            migrationBuilder.RenameColumn(
                name: "AgentId",
                table: "Agents",
                newName: "agent_id");

            migrationBuilder.RenameIndex(
                name: "IX_Agents_CompanyId",
                table: "Agents",
                newName: "IX_Agents_company_id");

            migrationBuilder.RenameColumn(
                name: "Phone",
                table: "Admins",
                newName: "phone");

            migrationBuilder.RenameColumn(
                name: "Password",
                table: "Admins",
                newName: "password");

            migrationBuilder.RenameColumn(
                name: "Email",
                table: "Admins",
                newName: "email");

            migrationBuilder.RenameColumn(
                name: "LastLogin",
                table: "Admins",
                newName: "last_login");

            migrationBuilder.RenameColumn(
                name: "FullName",
                table: "Admins",
                newName: "full_name");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "Admins",
                newName: "created_at");

            migrationBuilder.RenameColumn(
                name: "AdminId",
                table: "Admins",
                newName: "admin_id");

            migrationBuilder.AlterColumn<string>(
                name: "comment",
                table: "Reviews",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "subject",
                table: "Queries",
                type: "character varying(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "character varying(200)",
                oldMaxLength: 200,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "phone",
                table: "Queries",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "character varying(20)",
                oldMaxLength: 20,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "message",
                table: "Queries",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "created_by",
                table: "Policies",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "method",
                table: "Payments",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "character varying(50)",
                oldMaxLength: 50,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "description",
                table: "InsuranceTypes",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "type_id",
                table: "InsuranceTypes",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer")
                .Annotation("Relational:ColumnOrder", 1)
                .OldAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AddColumn<int>(
                name: "company_id",
                table: "InsuranceTypes",
                type: "integer",
                nullable: false,
                defaultValue: 0)
                .Annotation("Relational:ColumnOrder", 2);

            migrationBuilder.AddColumn<string>(
                name: "created_by",
                table: "InsuranceTypes",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AlterColumn<string>(
                name: "description",
                table: "InsurancePlans",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "phone",
                table: "Customers",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "character varying(20)",
                oldMaxLength: 20,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "city",
                table: "Customers",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "address",
                table: "Customers",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "family_info",
                table: "Customers",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "c_pancard",
                table: "Customers",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "character varying(20)",
                oldMaxLength: 20,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "c_adhar",
                table: "Customers",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "character varying(20)",
                oldMaxLength: 20,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "address",
                table: "Companies",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "license_number",
                table: "Companies",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "character varying(50)",
                oldMaxLength: 50,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "c_information",
                table: "Companies",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "c_agent",
                table: "Companies",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "pincode",
                table: "Agents",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "character varying(20)",
                oldMaxLength: 20,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "phone",
                table: "Agents",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "character varying(20)",
                oldMaxLength: 20,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "city",
                table: "Agents",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "address",
                table: "Agents",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "profile_photo",
                table: "Agents",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "license_number",
                table: "Agents",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "character varying(50)",
                oldMaxLength: 50,
                oldNullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_InsuranceTypes",
                table: "InsuranceTypes",
                columns: new[] { "type_id", "company_id" });

            migrationBuilder.CreateIndex(
                name: "IX_InsuranceTypes_company_id",
                table: "InsuranceTypes",
                column: "company_id");

            migrationBuilder.AddForeignKey(
                name: "FK_Agents_Companies_company_id",
                table: "Agents",
                column: "company_id",
                principalTable: "Companies",
                principalColumn: "company_id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_InsurancePlans_Agents_agent_id",
                table: "InsurancePlans",
                column: "agent_id",
                principalTable: "Agents",
                principalColumn: "agent_id");

            migrationBuilder.AddForeignKey(
                name: "FK_InsurancePlans_Companies_company_id",
                table: "InsurancePlans",
                column: "company_id",
                principalTable: "Companies",
                principalColumn: "company_id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_InsuranceTypes_Companies_company_id",
                table: "InsuranceTypes",
                column: "company_id",
                principalTable: "Companies",
                principalColumn: "company_id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Payments_Agents_received_by_agent",
                table: "Payments",
                column: "received_by_agent",
                principalTable: "Agents",
                principalColumn: "agent_id");

            migrationBuilder.AddForeignKey(
                name: "FK_Payments_Customers_customer_id",
                table: "Payments",
                column: "customer_id",
                principalTable: "Customers",
                principalColumn: "customer_id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Payments_Policies_policy_id",
                table: "Payments",
                column: "policy_id",
                principalTable: "Policies",
                principalColumn: "policy_id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Policies_Agents_agent_id",
                table: "Policies",
                column: "agent_id",
                principalTable: "Agents",
                principalColumn: "agent_id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Policies_Customers_customer_id",
                table: "Policies",
                column: "customer_id",
                principalTable: "Customers",
                principalColumn: "customer_id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Policies_InsurancePlans_plan_id_company_id",
                table: "Policies",
                columns: new[] { "plan_id", "company_id" },
                principalTable: "InsurancePlans",
                principalColumns: new[] { "plan_id", "company_id" },
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Queries_Customers_customer_id",
                table: "Queries",
                column: "customer_id",
                principalTable: "Customers",
                principalColumn: "customer_id");

            migrationBuilder.AddForeignKey(
                name: "FK_Reviews_Customers_customer_id",
                table: "Reviews",
                column: "customer_id",
                principalTable: "Customers",
                principalColumn: "customer_id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Reviews_InsurancePlans_plan_id_company_id",
                table: "Reviews",
                columns: new[] { "plan_id", "company_id" },
                principalTable: "InsurancePlans",
                principalColumns: new[] { "plan_id", "company_id" },
                onDelete: ReferentialAction.Restrict);
        }
    }
}
