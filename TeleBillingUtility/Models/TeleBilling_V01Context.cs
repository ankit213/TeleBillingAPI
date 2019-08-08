using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace TeleBillingUtility.Models
{
    public partial class TeleBilling_V01Context : DbContext
    {
        public TeleBilling_V01Context()
        {
        }

        public TeleBilling_V01Context(DbContextOptions<TeleBilling_V01Context> options)
            : base(options)
        {
        }

        public virtual DbSet<BillDelegate> BillDelegate { get; set; }
        public virtual DbSet<BillDetails> BillDetails { get; set; }
        public virtual DbSet<BillMaster> BillMaster { get; set; }
        public virtual DbSet<BillMasterServiceType> BillMasterServiceType { get; set; }
        public virtual DbSet<BillReImburse> BillReImburse { get; set; }
        public virtual DbSet<Configuration> Configuration { get; set; }
        public virtual DbSet<EmailTemplate> EmailTemplate { get; set; }
        public virtual DbSet<EmployeeBillMaster> EmployeeBillMaster { get; set; }
        public virtual DbSet<EmployeeBillServicePackage> EmployeeBillServicePackage { get; set; }
        public virtual DbSet<ExcelDetail> ExcelDetail { get; set; }
        public virtual DbSet<ExcelDetailPbx> ExcelDetailPbx { get; set; }
        public virtual DbSet<ExcelUploadLog> ExcelUploadLog { get; set; }
        public virtual DbSet<ExcelUploadLogPbx> ExcelUploadLogPbx { get; set; }
        public virtual DbSet<ExcelUploadLogServiceType> ExcelUploadLogServiceType { get; set; }
        public virtual DbSet<FixAssignType> FixAssignType { get; set; }
        public virtual DbSet<FixBillEmployeeStatus> FixBillEmployeeStatus { get; set; }
        public virtual DbSet<FixBillStatus> FixBillStatus { get; set; }
        public virtual DbSet<FixCallType> FixCallType { get; set; }
        public virtual DbSet<FixDevice> FixDevice { get; set; }
        public virtual DbSet<FixEmailTemplateTag> FixEmailTemplateTag { get; set; }
        public virtual DbSet<FixEmailTemplateType> FixEmailTemplateType { get; set; }
        public virtual DbSet<FixLineStatus> FixLineStatus { get; set; }
        public virtual DbSet<FixLineType> FixLineType { get; set; }
        public virtual DbSet<FixLogType> FixLogType { get; set; }
        public virtual DbSet<FixServiceType> FixServiceType { get; set; }
        public virtual DbSet<LogAuditTrial> LogAuditTrial { get; set; }
        public virtual DbSet<LogEmail> LogEmail { get; set; }
        public virtual DbSet<MappingExcel> MappingExcel { get; set; }
        public virtual DbSet<MappingExcelColumn> MappingExcelColumn { get; set; }
        public virtual DbSet<MappingExcelColumnPbx> MappingExcelColumnPbx { get; set; }
        public virtual DbSet<MappingExcelPbx> MappingExcelPbx { get; set; }
        public virtual DbSet<MappingServiceTypeField> MappingServiceTypeField { get; set; }
        public virtual DbSet<MappingServiceTypeFieldPbx> MappingServiceTypeFieldPbx { get; set; }
        public virtual DbSet<Memo> Memo { get; set; }
        public virtual DbSet<MemoBills> MemoBills { get; set; }
        public virtual DbSet<MstBusinessUnit> MstBusinessUnit { get; set; }
        public virtual DbSet<MstCostCenter> MstCostCenter { get; set; }
        public virtual DbSet<MstCountry> MstCountry { get; set; }
        public virtual DbSet<MstCurrency> MstCurrency { get; set; }
        public virtual DbSet<MstDepartment> MstDepartment { get; set; }
        public virtual DbSet<MstEmployee> MstEmployee { get; set; }
        public virtual DbSet<MstHandsetDetail> MstHandsetDetail { get; set; }
        public virtual DbSet<MstInternetDeviceDetail> MstInternetDeviceDetail { get; set; }
        public virtual DbSet<MstLink> MstLink { get; set; }
        public virtual DbSet<MstModule> MstModule { get; set; }
        public virtual DbSet<MstRequestAction> MstRequestAction { get; set; }
        public virtual DbSet<MstRole> MstRole { get; set; }
        public virtual DbSet<MstRoleRight> MstRoleRight { get; set; }
        public virtual DbSet<OperatorCallLog> OperatorCallLog { get; set; }
        public virtual DbSet<Provider> Provider { get; set; }
        public virtual DbSet<ProviderContactDetail> ProviderContactDetail { get; set; }
        public virtual DbSet<ProviderPackage> ProviderPackage { get; set; }
        public virtual DbSet<ProviderService> ProviderService { get; set; }
        public virtual DbSet<RequestTraceLog> RequestTraceLog { get; set; }
        public virtual DbSet<SkypeExcelDetail> SkypeExcelDetail { get; set; }
        public virtual DbSet<TelePhoneNumberAllocationPackage> TelePhoneNumberAllocationPackage { get; set; }
        public virtual DbSet<TelephoneNumber> TelephoneNumber { get; set; }
        public virtual DbSet<TelephoneNumberAllocation> TelephoneNumberAllocation { get; set; }
        public virtual DbSet<TransactionTypeSetting> TransactionTypeSetting { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. See http://go.microsoft.com/fwlink/?LinkId=723263 for guidance on storing connection strings.
                optionsBuilder.UseSqlServer("Server=WS-SRV-NET;Database=TeleBilling_V01;User ID=sa;Password=Admin@net;persist security info=True;Connect Timeout=200;Max Pool Size=200;Min Pool Size=5;Pooling=true;Connection Lifetime=300");
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasAnnotation("ProductVersion", "2.2.6-servicing-10079");

            modelBuilder.Entity<BillDelegate>(entity =>
            {
                entity.Property(e => e.CreatedDate).HasColumnType("datetime");

                entity.Property(e => e.CreatedDateInt).HasComputedColumnSql("((datepart(year,[CreatedDate])*(10000)+datepart(month,[CreatedDate])*(100))+datepart(day,[CreatedDate]))");

                entity.Property(e => e.UpdatedDate).HasColumnType("datetime");

                entity.Property(e => e.UpdatedDateInt).HasComputedColumnSql("((datepart(year,[UpdatedDate])*(10000)+datepart(month,[UpdatedDate])*(100))+datepart(day,[UpdatedDate]))");

                entity.HasOne(d => d.DelegateEmployee)
                    .WithMany(p => p.BillDelegateDelegateEmployee)
                    .HasForeignKey(d => d.DelegateEmployeeId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_BillDelegate_Mst_EmployeeDelegate");

                entity.HasOne(d => d.Employee)
                    .WithMany(p => p.BillDelegateEmployee)
                    .HasForeignKey(d => d.EmployeeId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_BillDelegate_Mst_Employee");
            });

            modelBuilder.Entity<BillDetails>(entity =>
            {
                entity.Property(e => e.CallAmount).HasColumnType("decimal(18, 2)");

                entity.Property(e => e.CallAssignedDate).HasColumnType("datetime");

                entity.Property(e => e.CallDate).HasColumnType("datetime");

                entity.Property(e => e.CallIdentifiedDate).HasColumnType("datetime");

                entity.Property(e => e.CallIwithInGroup).HasColumnName("CallIWithInGroup");

                entity.Property(e => e.CallerName).HasMaxLength(30);

                entity.Property(e => e.CallerNumber).HasMaxLength(20);

                entity.Property(e => e.CreatedDate).HasColumnType("datetime");

                entity.Property(e => e.CreatedDateInt).HasComputedColumnSql("((datepart(year,[CreatedDate])*(10000)+datepart(month,[CreatedDate])*(100))+datepart(day,[CreatedDate]))");

                entity.Property(e => e.Destination).HasMaxLength(50);

                entity.Property(e => e.EmployeeComment).HasMaxLength(50);

                entity.Property(e => e.ReceiverName).HasMaxLength(30);

                entity.Property(e => e.ReceiverNumber).HasMaxLength(20);

                entity.Property(e => e.SubscriptionType).HasMaxLength(50);

                entity.Property(e => e.TransType).HasMaxLength(50);

                entity.HasOne(d => d.BillMaster)
                    .WithMany(p => p.BillDetails)
                    .HasForeignKey(d => d.BillMasterId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_BillDetails_BillMaster");

                entity.HasOne(d => d.CallIdentificationTypeNavigation)
                    .WithMany(p => p.BillDetails)
                    .HasForeignKey(d => d.CallIdentificationType)
                    .HasConstraintName("FK_BillDetails_Fix_CallType");

                entity.HasOne(d => d.CallTransactionType)
                    .WithMany(p => p.BillDetails)
                    .HasForeignKey(d => d.CallTransactionTypeId)
                    .HasConstraintName("FK_BillDetails_TransactionTypeSetting");

                entity.HasOne(d => d.EmployeeBill)
                    .WithMany(p => p.BillDetails)
                    .HasForeignKey(d => d.EmployeeBillId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_BillDetails_EmployeeBillMaster");

                entity.HasOne(d => d.ServiceType)
                    .WithMany(p => p.BillDetails)
                    .HasForeignKey(d => d.ServiceTypeId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_BillDetails_Fix_ServiceType");
            });

            modelBuilder.Entity<BillMaster>(entity =>
            {
                entity.Property(e => e.BillAllocationDate).HasColumnType("datetime");

                entity.Property(e => e.BillAllocationDateInt).HasComputedColumnSql("((datepart(year,[BillAllocationDate])*(10000)+datepart(month,[BillAllocationDate])*(100))+datepart(day,[BillAllocationDate]))");

                entity.Property(e => e.BillAmount).HasColumnType("decimal(18, 2)");

                entity.Property(e => e.BillDueDate).HasColumnType("datetime");

                entity.Property(e => e.BillDueDateInt).HasComputedColumnSql("((datepart(year,[BillDueDate])*(10000)+datepart(month,[BillDueDate])*(100))+datepart(day,[BillDueDate]))");

                entity.Property(e => e.BillNumber)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.Property(e => e.CreatedDate).HasColumnType("datetime");

                entity.Property(e => e.CreatedDateInt).HasComputedColumnSql("((datepart(year,[CreatedDate])*(10000)+datepart(month,[CreatedDate])*(100))+datepart(day,[CreatedDate]))");

                entity.Property(e => e.UpdatedDate).HasColumnType("datetime");

                entity.Property(e => e.UpdatedDateInt).HasComputedColumnSql("((datepart(year,[UpdatedDate])*(10000)+datepart(month,[UpdatedDate])*(100))+datepart(day,[UpdatedDate]))");

                entity.HasOne(d => d.BillAllocatedByNavigation)
                    .WithMany(p => p.BillMaster)
                    .HasForeignKey(d => d.BillAllocatedBy)
                    .HasConstraintName("FK_BillMaster_Mst_Employee");

                entity.HasOne(d => d.BillStatus)
                    .WithMany(p => p.BillMaster)
                    .HasForeignKey(d => d.BillStatusId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_BillMaster_Fix_BillStatus");

                entity.HasOne(d => d.Currency)
                    .WithMany(p => p.BillMaster)
                    .HasForeignKey(d => d.CurrencyId)
                    .HasConstraintName("FK_BillMaster_BillMaster");

                entity.HasOne(d => d.Provider)
                    .WithMany(p => p.BillMaster)
                    .HasForeignKey(d => d.ProviderId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_BillMaster_Provider");
            });

            modelBuilder.Entity<BillMasterServiceType>(entity =>
            {
                entity.ToTable("BillMaster_ServiceType");

                entity.Property(e => e.UpdatedDate).HasColumnType("datetime");

                entity.Property(e => e.UpdatedDateInt).HasComputedColumnSql("((datepart(year,[UpdatedDate])*(10000)+datepart(month,[UpdatedDate])*(100))+datepart(day,[UpdatedDate]))");

                entity.HasOne(d => d.BillMaster)
                    .WithMany(p => p.BillMasterServiceType)
                    .HasForeignKey(d => d.BillMasterId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_BillMaster_ServiceType_BillMaster");

                entity.HasOne(d => d.ServiceType)
                    .WithMany(p => p.BillMasterServiceType)
                    .HasForeignKey(d => d.ServiceTypeId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_BillMaster_ServiceType_Fix_ServiceType");
            });

            modelBuilder.Entity<BillReImburse>(entity =>
            {
                entity.Property(e => e.ApprovalDate).HasColumnType("datetime");

                entity.Property(e => e.ApproveDateInt).HasComputedColumnSql("((datepart(year,[ApprovalDate])*(10000)+datepart(month,[ApprovalDate])*(100))+datepart(day,[ApprovalDate]))");

                entity.Property(e => e.CreatedDate).HasColumnType("datetime");

                entity.Property(e => e.CreatedDateInt).HasComputedColumnSql("((datepart(year,[CreatedDate])*(10000)+datepart(month,[CreatedDate])*(100))+datepart(day,[CreatedDate]))");

                entity.Property(e => e.Description).IsRequired();

                entity.Property(e => e.ReImbruseAmount).HasColumnType("decimal(18, 2)");

                entity.HasOne(d => d.BillMaster)
                    .WithMany(p => p.BillReImburse)
                    .HasForeignKey(d => d.BillMasterId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_BillReImburse_BillMaster");

                entity.HasOne(d => d.EmployeeBill)
                    .WithMany(p => p.BillReImburse)
                    .HasForeignKey(d => d.EmployeeBillId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_BillReImburse_EmployeeBillMaster");
            });

            modelBuilder.Entity<Configuration>(entity =>
            {
                entity.Property(e => e.CreatedDate).HasColumnType("datetime");

                entity.Property(e => e.CreatedDateInt).HasComputedColumnSql("((datepart(year,[CreatedDate])*(10000)+datepart(month,[CreatedDate])*(100))+datepart(day,[CreatedDate]))");

                entity.Property(e => e.NApprovedByLineManager).HasColumnName("N_ApprovedByLineManager");

                entity.Property(e => e.NBillAllocationToEmployee).HasColumnName("N_BillAllocationToEmployee");

                entity.Property(e => e.NBillDelegatesForIdentification).HasColumnName("N_BillDelegatesForIdentification");

                entity.Property(e => e.NChargeBill).HasColumnName("N_ChargeBill");

                entity.Property(e => e.NDelegatesBillForApproval).HasColumnName("N_DelegatesBillForApproval");

                entity.Property(e => e.NMemoApprovalRejection).HasColumnName("N_MemoApprovalRejection");

                entity.Property(e => e.NNewBillReceiveForApproval).HasColumnName("N_NewBillReceiveForApproval");

                entity.Property(e => e.NRejectedByLineManager).HasColumnName("N_RejectedByLineManager");

                entity.Property(e => e.NSendMemo).HasColumnName("N_SendMemo");

                entity.Property(e => e.REmployeeCallIdentificationInterval).HasColumnName("R_EmployeeCallIdentification_Interval");

                entity.Property(e => e.REmployeeCallIdentificationIsActive).HasColumnName("R_EmployeeCallIdentification_IsActive");

                entity.Property(e => e.RLinemanagerApprovalInterval).HasColumnName("R_LinemanagerApproval_Interval");

                entity.Property(e => e.RLinemanagerApprovalIsActive).HasColumnName("R_LinemanagerApproval_IsActive");

                entity.Property(e => e.UpdatedDate).HasColumnType("datetime");

                entity.Property(e => e.UpdatedDateInt).HasComputedColumnSql("((datepart(year,[UpdatedDate])*(10000)+datepart(month,[UpdatedDate])*(100))+datepart(day,[UpdatedDate]))");
            });

            modelBuilder.Entity<EmailTemplate>(entity =>
            {
                entity.Property(e => e.CreatedDate)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.CreatedDateInt).HasComputedColumnSql("((datepart(year,[CreatedDate])*(10000)+datepart(month,[CreatedDate])*(100))+datepart(day,[CreatedDate]))");

                entity.Property(e => e.EmailBcc)
                    .HasMaxLength(150)
                    .IsUnicode(false);

                entity.Property(e => e.EmailFrom)
                    .IsRequired()
                    .HasMaxLength(150)
                    .IsUnicode(false);

                entity.Property(e => e.EmailText).IsRequired();

                entity.Property(e => e.Subject)
                    .IsRequired()
                    .HasMaxLength(150);

                entity.Property(e => e.UpdatedDate)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.UpdatedDateInt).HasComputedColumnSql("((datepart(year,[UpdatedDate])*(10000)+datepart(month,[UpdatedDate])*(100))+datepart(day,[UpdatedDate]))");

                entity.HasOne(d => d.EmailTemplateType)
                    .WithMany(p => p.EmailTemplate)
                    .HasForeignKey(d => d.EmailTemplateTypeId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_EmailTemplate_Fix_EmailTemplateType");
            });

            modelBuilder.Entity<EmployeeBillMaster>(entity =>
            {
                entity.Property(e => e.ApprovalDate).HasColumnType("datetime");

                entity.Property(e => e.ApprovalDateInt).HasComputedColumnSql("((datepart(year,[ApprovalDate])*(10000)+datepart(month,[ApprovalDate])*(100))+datepart(day,[ApprovalDate]))");

                entity.Property(e => e.BillNumber)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.Property(e => e.CreatedDate).HasColumnType("datetime");

                entity.Property(e => e.CreatedDateInt).HasComputedColumnSql("((datepart(year,[CreatedDate])*(10000)+datepart(month,[CreatedDate])*(100))+datepart(day,[CreatedDate]))");

                entity.Property(e => e.TelephoneNumber).HasMaxLength(20);

                entity.Property(e => e.TotalBillAmount).HasColumnType("decimal(18, 2)");

                entity.Property(e => e.UpdatedDate).HasColumnType("datetime");

                entity.Property(e => e.UpdatedDateInt).HasComputedColumnSql("((datepart(year,[UpdatedDate])*(10000)+datepart(month,[UpdatedDate])*(100))+datepart(day,[UpdatedDate]))");

                entity.HasOne(d => d.BillMaster)
                    .WithMany(p => p.EmployeeBillMaster)
                    .HasForeignKey(d => d.BillMasterId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_EmployeeBillMaster_BillMaster");

                entity.HasOne(d => d.Currency)
                    .WithMany(p => p.EmployeeBillMaster)
                    .HasForeignKey(d => d.CurrencyId)
                    .HasConstraintName("FK_EmployeeBillMaster_Mst_Currency");

                entity.HasOne(d => d.EmpBusinessUnit)
                    .WithMany(p => p.EmployeeBillMaster)
                    .HasForeignKey(d => d.EmpBusinessUnitId)
                    .HasConstraintName("FK_EmployeeBillMaster_Mst_BusinessUnit");

                entity.HasOne(d => d.EmployeeBillStatusNavigation)
                    .WithMany(p => p.EmployeeBillMaster)
                    .HasForeignKey(d => d.EmployeeBillStatus)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_EmployeeBillMaster_Fix_BillEmployeeStatus");

                entity.HasOne(d => d.Employee)
                    .WithMany(p => p.EmployeeBillMasterEmployee)
                    .HasForeignKey(d => d.EmployeeId)
                    .HasConstraintName("FK_EmployeeBillMaster_Mst_Employee");

                entity.HasOne(d => d.Linemanager)
                    .WithMany(p => p.EmployeeBillMasterLinemanager)
                    .HasForeignKey(d => d.LinemanagerId)
                    .HasConstraintName("FK_EmployeeBillMaster_Mst_Employee1");

                entity.HasOne(d => d.MbileAssignTypeNavigation)
                    .WithMany(p => p.EmployeeBillMaster)
                    .HasForeignKey(d => d.MbileAssignType)
                    .HasConstraintName("FK_EmployeeBillMaster_Fix_AssignType");

                entity.HasOne(d => d.Provider)
                    .WithMany(p => p.EmployeeBillMaster)
                    .HasForeignKey(d => d.ProviderId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_EmployeeBillMaster_Provider");
            });

            modelBuilder.Entity<EmployeeBillServicePackage>(entity =>
            {
                entity.Property(e => e.BusinessIdentificationAmount).HasColumnType("decimal(18, 2)");

                entity.Property(e => e.BusinessTotalAmount).HasColumnType("decimal(18, 2)");

                entity.Property(e => e.DeductionAmount).HasColumnType("decimal(18, 2)");

                entity.Property(e => e.PersonalIdentificationAmount).HasColumnType("decimal(18, 2)");

                entity.Property(e => e.UpdatedDate).HasColumnType("datetime");

                entity.Property(e => e.UpdatedDateInt).HasComputedColumnSql("((datepart(year,[UpdatedDate])*(10000)+datepart(month,[UpdatedDate])*(100))+datepart(day,[UpdatedDate]))");

                entity.HasOne(d => d.EmployeeBill)
                    .WithMany(p => p.EmployeeBillServicePackage)
                    .HasForeignKey(d => d.EmployeeBillId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_EmployeeBillServicePackage_EmployeeBillMaster");

                entity.HasOne(d => d.Package)
                    .WithMany(p => p.EmployeeBillServicePackage)
                    .HasForeignKey(d => d.PackageId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_EmployeeBillServicePackage_ProviderPackage");

                entity.HasOne(d => d.ServiceType)
                    .WithMany(p => p.EmployeeBillServicePackage)
                    .HasForeignKey(d => d.ServiceTypeId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_EmployeeBillServicePackage_Fix_ServiceType");
            });

            modelBuilder.Entity<ExcelDetail>(entity =>
            {
                entity.Property(e => e.Bandwidth)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.BusinessUnit)
                    .HasMaxLength(200)
                    .IsUnicode(false);

                entity.Property(e => e.CallAmount).HasColumnType("decimal(18, 2)");

                entity.Property(e => e.CallDate).HasColumnType("datetime");

                entity.Property(e => e.CallerName).HasMaxLength(150);

                entity.Property(e => e.CallerNumber)
                    .HasMaxLength(20)
                    .IsUnicode(false);

                entity.Property(e => e.CostCentre)
                    .HasMaxLength(200)
                    .IsUnicode(false);

                entity.Property(e => e.FinalAnnualChargesKd)
                    .HasColumnName("FinalAnnualChargesKD")
                    .HasColumnType("decimal(18, 2)");

                entity.Property(e => e.GroupDetail)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.InitialDiscountedAnnualPriceKd)
                    .HasColumnName("InitialDiscountedAnnualPriceKD")
                    .HasColumnType("decimal(18, 2)");

                entity.Property(e => e.InitialDiscountedMonthlyPriceKd)
                    .HasColumnName("InitialDiscountedMonthlyPriceKD")
                    .HasColumnType("decimal(18, 2)");

                entity.Property(e => e.InitialDiscountedSavingMonthlyKd)
                    .HasColumnName("InitialDiscountedSavingMonthlyKD")
                    .HasColumnType("decimal(18, 2)");

                entity.Property(e => e.InitialDiscountedSavingYearlyKd)
                    .HasColumnName("InitialDiscountedSavingYearlyKD")
                    .HasColumnType("decimal(18, 2)");

                entity.Property(e => e.MonthlyPrice).HasColumnType("decimal(18, 3)");

                entity.Property(e => e.ReceiverName).HasMaxLength(150);

                entity.Property(e => e.ReceiverNumber)
                    .HasMaxLength(20)
                    .IsUnicode(false);

                entity.Property(e => e.ServiceDetail)
                    .HasMaxLength(200)
                    .IsUnicode(false);

                entity.Property(e => e.SiteName)
                    .HasMaxLength(150)
                    .IsUnicode(false);

                entity.Property(e => e.SubscriptionType).HasMaxLength(50);

                entity.HasOne(d => d.AssignTypeNavigation)
                    .WithMany(p => p.ExcelDetail)
                    .HasForeignKey(d => d.AssignType)
                    .HasConstraintName("FK_ExcelDetail_Fix_AssignType");

                entity.HasOne(d => d.BusinessUnitNavigation)
                    .WithMany(p => p.ExcelDetail)
                    .HasForeignKey(d => d.BusinessUnitId)
                    .HasConstraintName("FK_ExcelDetail_Mst_BusinessUnit");

                entity.HasOne(d => d.CallTransactionType)
                    .WithMany(p => p.ExcelDetail)
                    .HasForeignKey(d => d.CallTransactionTypeId)
                    .HasConstraintName("FK_ExcelDetail_Transaction_Type_Setting");

                entity.HasOne(d => d.CostCenter)
                    .WithMany(p => p.ExcelDetail)
                    .HasForeignKey(d => d.CostCenterId)
                    .HasConstraintName("FK_ExcelDetail_Mst_CostCenter");

                entity.HasOne(d => d.Currency)
                    .WithMany(p => p.ExcelDetail)
                    .HasForeignKey(d => d.CurrencyId)
                    .HasConstraintName("FK_ExcelDetail_Mst_Currency");

                entity.HasOne(d => d.Employee)
                    .WithMany(p => p.ExcelDetail)
                    .HasForeignKey(d => d.EmployeeId)
                    .HasConstraintName("FK_ExcelDetail_Mst_Employee");

                entity.HasOne(d => d.ServiceType)
                    .WithMany(p => p.ExcelDetail)
                    .HasForeignKey(d => d.ServiceTypeId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_ExcelDetail_Fix_ServiceType");
            });

            modelBuilder.Entity<ExcelDetailPbx>(entity =>
            {
                entity.Property(e => e.Band)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.CallAmount).HasColumnType("decimal(18, 2)");

                entity.Property(e => e.CallDate).HasColumnType("datetime");

                entity.Property(e => e.CallType)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.ClassificationCode)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.CodeNumber)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.ConnectingParty)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.DestinationType)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.DistantNumber)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.Name1)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.Name2)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.Name3)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.Name4)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.OtherParty)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.Place)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.Rate)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.HasOne(d => d.ExcelUploadLog)
                    .WithMany(p => p.ExcelDetailPbx)
                    .HasForeignKey(d => d.ExcelUploadLogId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_ExcelDetailPbx_ExcelUploadLogPbx");
            });

            modelBuilder.Entity<ExcelUploadLog>(entity =>
            {
                entity.Property(e => e.ExcelFileName)
                    .IsRequired()
                    .IsUnicode(false);

                entity.Property(e => e.FileNameGuid)
                    .IsRequired()
                    .IsUnicode(false);

                entity.Property(e => e.IsPbxupload).HasColumnName("IsPBXUpload");

                entity.Property(e => e.TotalImportedBillAmount).HasColumnType("decimal(18, 0)");

                entity.Property(e => e.UpdatedDate).HasColumnType("datetime");

                entity.Property(e => e.UpdatedDateInt).HasComputedColumnSql("((datepart(year,[UpdatedDate])*(10000)+datepart(month,[UpdatedDate])*(100))+datepart(day,[UpdatedDate]))");

                entity.Property(e => e.UploadDate).HasColumnType("datetime");

                entity.Property(e => e.UploadDateInt).HasComputedColumnSql("((datepart(year,[UploadDate])*(10000)+datepart(month,[UploadDate])*(100))+datepart(day,[UploadDate]))");

                entity.HasOne(d => d.Provider)
                    .WithMany(p => p.ExcelUploadLog)
                    .HasForeignKey(d => d.ProviderId)
                    .HasConstraintName("FK_ExcelUploadLog_Provider");
            });

            modelBuilder.Entity<ExcelUploadLogPbx>(entity =>
            {
                entity.Property(e => e.ExcelFileName)
                    .IsRequired()
                    .IsUnicode(false);

                entity.Property(e => e.FileNameGuid)
                    .IsRequired()
                    .IsUnicode(false);

                entity.Property(e => e.TotalImportedBillAmount).HasColumnType("decimal(18, 0)");

                entity.Property(e => e.UpdatedDate).HasColumnType("datetime");

                entity.Property(e => e.UpdatedDateInt).HasComputedColumnSql("((datepart(year,[UpdatedDate])*(10000)+datepart(month,[UpdatedDate])*(100))+datepart(day,[UpdatedDate]))");

                entity.Property(e => e.UploadDate).HasColumnType("datetime");

                entity.Property(e => e.UploadDateInt).HasComputedColumnSql("((datepart(year,[UploadDate])*(10000)+datepart(month,[UploadDate])*(100))+datepart(day,[UploadDate]))");

                entity.HasOne(d => d.Device)
                    .WithMany(p => p.ExcelUploadLogPbx)
                    .HasForeignKey(d => d.DeviceId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_ExcelUploadLogPbx_Fix_Device");
            });

            modelBuilder.Entity<ExcelUploadLogServiceType>(entity =>
            {
                entity.ToTable("ExcelUploadLog_ServiceType");

                entity.Property(e => e.UpdatedDate).HasColumnType("datetime");

                entity.Property(e => e.UpdatedDateInt).HasComputedColumnSql("((datepart(year,[UpdatedDate])*(10000)+datepart(month,[UpdatedDate])*(100))+datepart(day,[UpdatedDate]))");

                entity.HasOne(d => d.ExceluploadLog)
                    .WithMany(p => p.ExcelUploadLogServiceType)
                    .HasForeignKey(d => d.ExceluploadLogId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_ExcelUploadLog_ServiceType_ExcelUploadLog");

                entity.HasOne(d => d.ServiceType)
                    .WithMany(p => p.ExcelUploadLogServiceType)
                    .HasForeignKey(d => d.ServiceTypeId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_ExcelUploadLog_ServiceType_Fix_ServiceType");
            });

            modelBuilder.Entity<FixAssignType>(entity =>
            {
                entity.ToTable("Fix_AssignType");

                entity.Property(e => e.CreatedDate).HasColumnType("datetime");

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(50);
            });

            modelBuilder.Entity<FixBillEmployeeStatus>(entity =>
            {
                entity.ToTable("Fix_BillEmployeeStatus");

                entity.Property(e => e.Id).ValueGeneratedNever();

                entity.Property(e => e.CreatedDate).HasColumnType("datetime");

                entity.Property(e => e.CreatedDateInt).HasComputedColumnSql("((datepart(year,[CreatedDate])*(10000)+datepart(month,[CreatedDate])*(100))+datepart(day,[CreatedDate]))");

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(50);
            });

            modelBuilder.Entity<FixBillStatus>(entity =>
            {
                entity.ToTable("Fix_BillStatus");

                entity.Property(e => e.Id).ValueGeneratedNever();

                entity.Property(e => e.CreatedDate).HasColumnType("datetime");

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(50);
            });

            modelBuilder.Entity<FixCallType>(entity =>
            {
                entity.ToTable("Fix_CallType");

                entity.Property(e => e.Id).ValueGeneratedNever();

                entity.Property(e => e.CreatedDate).HasColumnType("datetime");

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(50);
            });

            modelBuilder.Entity<FixDevice>(entity =>
            {
                entity.ToTable("Fix_Device");

                entity.Property(e => e.CreatedDate).HasColumnType("datetime");

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(80);
            });

            modelBuilder.Entity<FixEmailTemplateTag>(entity =>
            {
                entity.ToTable("Fix_EmailTemplateTag");

                entity.Property(e => e.CreatedDate).HasColumnType("datetime");

                entity.Property(e => e.TemplateTag)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.Property(e => e.TemplateText)
                    .IsRequired()
                    .HasMaxLength(50);
            });

            modelBuilder.Entity<FixEmailTemplateType>(entity =>
            {
                entity.ToTable("Fix_EmailTemplateType");

                entity.Property(e => e.CreatedDate).HasColumnType("datetime");

                entity.Property(e => e.TemplateType)
                    .IsRequired()
                    .HasMaxLength(50);
            });

            modelBuilder.Entity<FixLineStatus>(entity =>
            {
                entity.ToTable("Fix_LineStatus");

                entity.Property(e => e.Id).ValueGeneratedNever();

                entity.Property(e => e.CreatedDate).HasColumnType("datetime");

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(50);
            });

            modelBuilder.Entity<FixLineType>(entity =>
            {
                entity.ToTable("Fix_LineType");

                entity.Property(e => e.CreatedDate).HasColumnType("datetime");

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(50);
            });

            modelBuilder.Entity<FixLogType>(entity =>
            {
                entity.HasKey(e => e.LogTypeId);

                entity.ToTable("Fix_LogType");

                entity.Property(e => e.LogText).IsRequired();

                entity.Property(e => e.TypeName)
                    .IsRequired()
                    .HasMaxLength(20);
            });

            modelBuilder.Entity<FixServiceType>(entity =>
            {
                entity.ToTable("Fix_ServiceType");

                entity.Property(e => e.CreatedDate).HasColumnType("datetime");

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(50);
            });

            modelBuilder.Entity<LogAuditTrial>(entity =>
            {
                entity.HasKey(e => e.AuditTrialLogId)
                    .HasName("PK_AuditTrialLog");

                entity.ToTable("Log_AuditTrial");

                entity.Property(e => e.AuditDate).HasColumnType("datetime");

                entity.Property(e => e.AuditDateInt).HasComputedColumnSql("((datepart(year,[AuditDate])*(10000)+datepart(month,[AuditDate])*(100))+datepart(day,[AuditDate]))");

                entity.Property(e => e.Browser).HasMaxLength(150);

                entity.Property(e => e.Description).IsRequired();

                entity.Property(e => e.Ipaddress)
                    .HasColumnName("IPAddress")
                    .HasMaxLength(50);

                entity.Property(e => e.Version).HasMaxLength(50);

                entity.HasOne(d => d.LogType)
                    .WithMany(p => p.LogAuditTrial)
                    .HasForeignKey(d => d.LogTypeId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_AuditTrialLog_Fix_LogType");
            });

            modelBuilder.Entity<LogEmail>(entity =>
            {
                entity.ToTable("Log_Email");

                entity.Property(e => e.CreatedDate)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.CreatedDateInt).HasComputedColumnSql("((datepart(year,[CreatedDate])*(10000)+datepart(month,[CreatedDate])*(100))+datepart(day,[CreatedDate]))");

                entity.Property(e => e.EmailBcc)
                    .IsRequired()
                    .HasColumnName("EmailBCC")
                    .HasMaxLength(150)
                    .IsUnicode(false);

                entity.Property(e => e.EmailFrom)
                    .IsRequired()
                    .HasMaxLength(150)
                    .IsUnicode(false);

                entity.Property(e => e.EmailText).IsRequired();

                entity.Property(e => e.EmailTo)
                    .IsRequired()
                    .HasMaxLength(150)
                    .IsUnicode(false);

                entity.Property(e => e.SendDate).HasColumnType("datetime");

                entity.Property(e => e.SendDateInt).HasComputedColumnSql("((datepart(year,[SendDate])*(10000)+datepart(month,[SendDate])*(100))+datepart(day,[SendDate]))");

                entity.Property(e => e.Subject)
                    .IsRequired()
                    .HasMaxLength(50);
            });

            modelBuilder.Entity<MappingExcel>(entity =>
            {
                entity.Property(e => e.CreatedDate)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.CreatedDateInt).HasComputedColumnSql("((datepart(year,[CreatedDate])*(10000)+datepart(month,[CreatedDate])*(100))+datepart(day,[CreatedDate]))");

                entity.Property(e => e.ExcelColumnNameForTitle)
                    .HasMaxLength(6)
                    .IsUnicode(false);

                entity.Property(e => e.ExcelReadingColumn)
                    .IsRequired()
                    .HasMaxLength(6)
                    .IsUnicode(false);

                entity.Property(e => e.TitleName)
                    .HasMaxLength(150)
                    .IsUnicode(false);

                entity.Property(e => e.UpdatedDate)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.UpdatedDateInt).HasComputedColumnSql("((datepart(year,[UpdatedDate])*(10000)+datepart(month,[UpdatedDate])*(100))+datepart(day,[UpdatedDate]))");

                entity.HasOne(d => d.Provider)
                    .WithMany(p => p.MappingExcel)
                    .HasForeignKey(d => d.ProviderId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_MappingExcel_Provider");

                entity.HasOne(d => d.ServiceType)
                    .WithMany(p => p.MappingExcel)
                    .HasForeignKey(d => d.ServiceTypeId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_MappingExcel_Fix_ServiceType");
            });

            modelBuilder.Entity<MappingExcelColumn>(entity =>
            {
                entity.Property(e => e.ExcelcolumnName)
                    .IsRequired()
                    .HasMaxLength(10)
                    .IsUnicode(false);

                entity.Property(e => e.FormatField)
                    .HasMaxLength(100)
                    .IsUnicode(false);

                entity.HasOne(d => d.MappingExcel)
                    .WithMany(p => p.MappingExcelColumn)
                    .HasForeignKey(d => d.MappingExcelId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_MappingExcelColumn_MappingExcel");

                entity.HasOne(d => d.MappingServiceTypeField)
                    .WithMany(p => p.MappingExcelColumn)
                    .HasForeignKey(d => d.MappingServiceTypeFieldId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_MappingExcelColumn_MappingServiceTypeField");
            });

            modelBuilder.Entity<MappingExcelColumnPbx>(entity =>
            {
                entity.ToTable("MappingExcelColumn_PBX");

                entity.Property(e => e.ExcelcolumnName)
                    .IsRequired()
                    .HasMaxLength(10)
                    .IsUnicode(false);

                entity.Property(e => e.FormatField)
                    .HasMaxLength(100)
                    .IsUnicode(false);

                entity.HasOne(d => d.MappingExcel)
                    .WithMany(p => p.MappingExcelColumnPbx)
                    .HasForeignKey(d => d.MappingExcelId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_MappingExcelColumn_PBX_MappingExcel_PBX");

                entity.HasOne(d => d.MappingServiceTypeField)
                    .WithMany(p => p.MappingExcelColumnPbx)
                    .HasForeignKey(d => d.MappingServiceTypeFieldId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_MappingExcelColumn_PBX_MappingServiceTypeField_PBX");
            });

            modelBuilder.Entity<MappingExcelPbx>(entity =>
            {
                entity.ToTable("MappingExcel_PBX");

                entity.Property(e => e.CreatedDate)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.CreatedDateInt).HasComputedColumnSql("((datepart(year,[CreatedDate])*(10000)+datepart(month,[CreatedDate])*(100))+datepart(day,[CreatedDate]))");

                entity.Property(e => e.ExcelColumnNameForTitle)
                    .HasMaxLength(6)
                    .IsUnicode(false);

                entity.Property(e => e.ExcelReadingColumn)
                    .IsRequired()
                    .HasMaxLength(6)
                    .IsUnicode(false);

                entity.Property(e => e.TitleName)
                    .HasMaxLength(150)
                    .IsUnicode(false);

                entity.Property(e => e.UpdatedDate)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.UpdatedDateInt).HasComputedColumnSql("((datepart(year,[UpdatedDate])*(10000)+datepart(month,[UpdatedDate])*(100))+datepart(day,[UpdatedDate]))");

                entity.HasOne(d => d.Device)
                    .WithMany(p => p.MappingExcelPbx)
                    .HasForeignKey(d => d.DeviceId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_MappingExcel_PBX_Fix_Device");
            });

            modelBuilder.Entity<MappingServiceTypeField>(entity =>
            {
                entity.Property(e => e.DbcolumnName)
                    .IsRequired()
                    .HasColumnName("DBColumnName")
                    .HasMaxLength(60)
                    .IsUnicode(false);

                entity.Property(e => e.DbtableName)
                    .IsRequired()
                    .HasColumnName("DBTableName")
                    .HasMaxLength(60)
                    .IsUnicode(false);

                entity.Property(e => e.DisplayFieldName)
                    .IsRequired()
                    .HasMaxLength(60)
                    .IsUnicode(false);

                entity.HasOne(d => d.ServiceType)
                    .WithMany(p => p.MappingServiceTypeField)
                    .HasForeignKey(d => d.ServiceTypeId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_MappingServiceTypeField_Fix_ServiceType");
            });

            modelBuilder.Entity<MappingServiceTypeFieldPbx>(entity =>
            {
                entity.ToTable("MappingServiceTypeField_PBX");

                entity.Property(e => e.DbcolumnName)
                    .IsRequired()
                    .HasColumnName("DBColumnName")
                    .HasMaxLength(30)
                    .IsUnicode(false);

                entity.Property(e => e.DbtableName)
                    .IsRequired()
                    .HasColumnName("DBTableName")
                    .HasMaxLength(30)
                    .IsUnicode(false);

                entity.Property(e => e.DisplayFieldName)
                    .IsRequired()
                    .HasMaxLength(30)
                    .IsUnicode(false);

                entity.HasOne(d => d.Device)
                    .WithMany(p => p.MappingServiceTypeFieldPbx)
                    .HasForeignKey(d => d.DeviceId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_MappingServiceTypeField_PBX_Fix_Device");
            });

            modelBuilder.Entity<Memo>(entity =>
            {
                entity.Property(e => e.ApprovedDate).HasColumnType("datetime");

                entity.Property(e => e.ApprovedDateInt).HasComputedColumnSql("((datepart(year,[ApprovedDate])*(10000)+datepart(month,[ApprovedDate])*(100))+datepart(day,[ApprovedDate]))");

                entity.Property(e => e.Bank).HasMaxLength(100);

                entity.Property(e => e.CreatedDate).HasColumnType("datetime");

                entity.Property(e => e.CreatedDateInt).HasComputedColumnSql("((datepart(year,[CreatedDate])*(10000)+datepart(month,[CreatedDate])*(100))+datepart(day,[CreatedDate]))");

                entity.Property(e => e.Date).HasColumnType("datetime");

                entity.Property(e => e.Ibancode)
                    .HasColumnName("IBANCode")
                    .HasMaxLength(100);

                entity.Property(e => e.RefrenceNo)
                    .IsRequired()
                    .HasMaxLength(200);

                entity.Property(e => e.Subject)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.Property(e => e.Swiftcode)
                    .HasColumnName("SWIFTCode")
                    .HasMaxLength(100);

                entity.Property(e => e.TotalAmount).HasColumnType("decimal(18, 0)");

                entity.Property(e => e.UpdatedDate).HasColumnType("datetime");

                entity.Property(e => e.UpdatedDateInt).HasComputedColumnSql("((datepart(year,[UpdatedDate])*(10000)+datepart(month,[UpdatedDate])*(100))+datepart(day,[UpdatedDate]))");

                entity.HasOne(d => d.Provider)
                    .WithMany(p => p.Memo)
                    .HasForeignKey(d => d.ProviderId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Memo_Provider");
            });

            modelBuilder.Entity<MemoBills>(entity =>
            {
                entity.Property(e => e.CreatedDate).HasColumnType("datetime");

                entity.Property(e => e.CreatedDateInt).HasComputedColumnSql("((datepart(year,[CreatedDate])*(10000)+datepart(month,[CreatedDate])*(100))+datepart(day,[CreatedDate]))");

                entity.HasOne(d => d.Bill)
                    .WithMany(p => p.MemoBills)
                    .HasForeignKey(d => d.BillId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_MemoBills_BillMaster");

                entity.HasOne(d => d.Memo)
                    .WithMany(p => p.MemoBills)
                    .HasForeignKey(d => d.MemoId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_MemoBills_Memo");
            });

            modelBuilder.Entity<MstBusinessUnit>(entity =>
            {
                entity.ToTable("Mst_BusinessUnit");

                entity.Property(e => e.CreatedDate).HasColumnType("datetime");

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(50);
            });

            modelBuilder.Entity<MstCostCenter>(entity =>
            {
                entity.ToTable("Mst_CostCenter");

                entity.Property(e => e.CostCenterCode)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.Property(e => e.CreatedDate).HasColumnType("datetime");

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.HasOne(d => d.BusinessUnit)
                    .WithMany(p => p.MstCostCenter)
                    .HasForeignKey(d => d.BusinessUnitid)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Mst_CostCenter_Mst_BusinessUnit");
            });

            modelBuilder.Entity<MstCountry>(entity =>
            {
                entity.ToTable("Mst_Country");

                entity.Property(e => e.CreatedDate).HasColumnType("datetime");

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.HasOne(d => d.Currency)
                    .WithMany(p => p.MstCountry)
                    .HasForeignKey(d => d.CurrencyId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Mst_Country_Mst_Currency");
            });

            modelBuilder.Entity<MstCurrency>(entity =>
            {
                entity.ToTable("Mst_Currency");

                entity.Property(e => e.Code)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.Property(e => e.CreatedDate).HasColumnType("datetime");

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(50);
            });

            modelBuilder.Entity<MstDepartment>(entity =>
            {
                entity.ToTable("Mst_Department");

                entity.Property(e => e.CreatedDate).HasColumnType("datetime");

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.HasOne(d => d.BusinessUnit)
                    .WithMany(p => p.MstDepartment)
                    .HasForeignKey(d => d.BusinessUnitId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Mst_Department_Mst_BusinessUnit");
            });

            modelBuilder.Entity<MstEmployee>(entity =>
            {
                entity.HasKey(e => e.UserId);

                entity.ToTable("Mst_Employee");

                entity.Property(e => e.CreatedDate)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.CreatedDateInt).HasComputedColumnSql("((datepart(year,[CreatedDate])*(10000)+datepart(month,[CreatedDate])*(100))+datepart(day,[CreatedDate]))");

                entity.Property(e => e.Designation).HasMaxLength(80);

                entity.Property(e => e.EmailId)
                    .HasMaxLength(150)
                    .IsUnicode(false);

                entity.Property(e => e.EmpPfnumber)
                    .IsRequired()
                    .HasColumnName("EmpPFNumber")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.ExtensionNumber).HasMaxLength(50);

                entity.Property(e => e.FullName)
                    .IsRequired()
                    .HasMaxLength(80);

                entity.Property(e => e.Password)
                    .IsRequired()
                    .HasMaxLength(25);

                entity.Property(e => e.UpdatedDate)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.UpdatedDateInt).HasComputedColumnSql("((datepart(year,[UpdatedDate])*(10000)+datepart(month,[UpdatedDate])*(100))+datepart(day,[UpdatedDate]))");

                entity.HasOne(d => d.CostCenter)
                    .WithMany(p => p.MstEmployee)
                    .HasForeignKey(d => d.CostCenterId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Mst_Employee_Mst_CostCenter");

                entity.HasOne(d => d.Department)
                    .WithMany(p => p.MstEmployee)
                    .HasForeignKey(d => d.DepartmentId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Mst_Employee_Mst_Department");

                entity.HasOne(d => d.LineManager)
                    .WithMany(p => p.InverseLineManager)
                    .HasForeignKey(d => d.LineManagerId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Mst_EmployeeLinemanager_Mst_Employee");
            });

            modelBuilder.Entity<MstHandsetDetail>(entity =>
            {
                entity.ToTable("Mst_HandsetDetail");

                entity.Property(e => e.CreatedDate)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.CreatedDateInt).HasComputedColumnSql("((datepart(year,[CreatedDate])*(10000)+datepart(month,[CreatedDate])*(100))+datepart(day,[CreatedDate]))");

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(150);

                entity.Property(e => e.UpdatedDate).HasColumnType("datetime");

                entity.Property(e => e.UpdatedDateInt).HasComputedColumnSql("((datepart(year,[UpdatedDate])*(10000)+datepart(month,[UpdatedDate])*(100))+datepart(day,[UpdatedDate]))");
            });

            modelBuilder.Entity<MstInternetDeviceDetail>(entity =>
            {
                entity.ToTable("Mst_InternetDeviceDetail");

                entity.Property(e => e.CreatedDate)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.CreatedDateInt).HasComputedColumnSql("((datepart(year,[CreatedDate])*(10000)+datepart(month,[CreatedDate])*(100))+datepart(day,[CreatedDate]))");

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(150);

                entity.Property(e => e.UpdatedDate).HasColumnType("datetime");

                entity.Property(e => e.UpdatedDateInt).HasComputedColumnSql("((datepart(year,[UpdatedDate])*(10000)+datepart(month,[UpdatedDate])*(100))+datepart(day,[UpdatedDate]))");
            });

            modelBuilder.Entity<MstLink>(entity =>
            {
                entity.HasKey(e => e.LinkId);

                entity.ToTable("Mst_Link");

                entity.Property(e => e.CreatedDate)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.CreatedDateInt).HasComputedColumnSql("((datepart(year,[CreatedDate])*(10000)+datepart(month,[CreatedDate])*(100))+datepart(day,[CreatedDate]))");

                entity.Property(e => e.RouteLink)
                    .IsRequired()
                    .HasMaxLength(150)
                    .IsUnicode(false);

                entity.Property(e => e.Title)
                    .IsRequired()
                    .HasMaxLength(150)
                    .IsUnicode(false);

                entity.Property(e => e.UpdatedBy).HasDefaultValueSql("((0))");

                entity.Property(e => e.UpdatedDate)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.UpdatedDateInt).HasComputedColumnSql("((datepart(year,[UpdatedDate])*(10000)+datepart(month,[UpdatedDate])*(100))+datepart(day,[UpdatedDate]))");

                entity.HasOne(d => d.Module)
                    .WithMany(p => p.MstLink)
                    .HasForeignKey(d => d.ModuleId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Mst_Link_Mst_Module");
            });

            modelBuilder.Entity<MstModule>(entity =>
            {
                entity.HasKey(e => e.ModuleId);

                entity.ToTable("Mst_Module");

                entity.Property(e => e.CreatedDate)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.CreatedDateInt).HasComputedColumnSql("((datepart(year,[CreatedDate])*(10000)+datepart(month,[CreatedDate])*(100))+datepart(day,[CreatedDate]))");

                entity.Property(e => e.IconName)
                    .IsRequired()
                    .HasMaxLength(100)
                    .IsUnicode(false);

                entity.Property(e => e.ModuleName)
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<MstRequestAction>(entity =>
            {
                entity.HasKey(e => e.RequestId);

                entity.ToTable("Mst_RequestAction");

                entity.Property(e => e.Text)
                    .IsRequired()
                    .HasMaxLength(50);
            });

            modelBuilder.Entity<MstRole>(entity =>
            {
                entity.HasKey(e => e.RoleId);

                entity.ToTable("Mst_Role");

                entity.Property(e => e.CreatedDate)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.CreatedDateInt).HasComputedColumnSql("((datepart(year,[CreatedDate])*(10000)+datepart(month,[CreatedDate])*(100))+datepart(day,[CreatedDate]))");

                entity.Property(e => e.RoleName)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.Property(e => e.UpdatedDate)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.UpdatedDateInt).HasComputedColumnSql("((datepart(year,[UpdatedDate])*(10000)+datepart(month,[UpdatedDate])*(100))+datepart(day,[UpdatedDate]))");
            });

            modelBuilder.Entity<MstRoleRight>(entity =>
            {
                entity.HasKey(e => e.RoleRightId);

                entity.ToTable("Mst_RoleRight");

                entity.Property(e => e.CreatedDate)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.CreatedDateInt).HasComputedColumnSql("((datepart(year,[CreatedDate])*(10000)+datepart(month,[CreatedDate])*(100))+datepart(day,[CreatedDate]))");
            });

            modelBuilder.Entity<OperatorCallLog>(entity =>
            {
                entity.Property(e => e.CallDate).HasColumnType("datetime");

                entity.Property(e => e.CallDateInt).HasComputedColumnSql("((datepart(year,[CallDate])*(10000)+datepart(month,[CallDate])*(100))+datepart(day,[CallDate]))");

                entity.Property(e => e.CreatedDate).HasColumnType("datetime");

                entity.Property(e => e.CreatedDateInt).HasComputedColumnSql("((datepart(year,[CreatedDate])*(10000)+datepart(month,[CreatedDate])*(100))+datepart(day,[CreatedDate]))");

                entity.Property(e => e.DialedNumber)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.Property(e => e.EmpPfnumber)
                    .IsRequired()
                    .HasColumnName("EmpPFNumber")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.ExtensionNumber).HasMaxLength(50);

                entity.Property(e => e.UpdatedDate).HasColumnType("datetime");

                entity.Property(e => e.UpdatedDateInt).HasComputedColumnSql("((datepart(year,[UpdatedDate])*(10000)+datepart(month,[UpdatedDate])*(100))+datepart(day,[UpdatedDate]))");

                entity.HasOne(d => d.CallType)
                    .WithMany(p => p.OperatorCallLog)
                    .HasForeignKey(d => d.CallTypeId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_OperatorCallLog_Fix_CallType");

                entity.HasOne(d => d.Employee)
                    .WithMany(p => p.OperatorCallLog)
                    .HasForeignKey(d => d.EmployeeId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_OperatorCallLog_Mst_Employee");

                entity.HasOne(d => d.Provider)
                    .WithMany(p => p.OperatorCallLog)
                    .HasForeignKey(d => d.ProviderId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_OperatorCallLog_Provider");
            });

            modelBuilder.Entity<Provider>(entity =>
            {
                entity.Property(e => e.AccountNumber)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.Property(e => e.Bank)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.Property(e => e.ContractNumber)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.Property(e => e.CreatedDate)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.CreatedDateInt).HasComputedColumnSql("((datepart(year,[CreatedDate])*(10000)+datepart(month,[CreatedDate])*(100))+datepart(day,[CreatedDate]))");

                entity.Property(e => e.Ibancode)
                    .IsRequired()
                    .HasColumnName("IBANCode")
                    .HasMaxLength(100);

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.Property(e => e.ProviderEmail)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.Property(e => e.Swiftcode)
                    .IsRequired()
                    .HasColumnName("SWIFTCode")
                    .HasMaxLength(100);

                entity.Property(e => e.UpdatedDate)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.UpdatedDateInt).HasComputedColumnSql("((datepart(year,[UpdatedDate])*(10000)+datepart(month,[UpdatedDate])*(100))+datepart(day,[UpdatedDate]))");

                entity.HasOne(d => d.Country)
                    .WithMany(p => p.Provider)
                    .HasForeignKey(d => d.CountryId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Provider_Mst_Country");
            });

            modelBuilder.Entity<ProviderContactDetail>(entity =>
            {
                entity.Property(e => e.ContactNumbers)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.Property(e => e.Email)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.Property(e => e.Position).HasMaxLength(50);

                entity.Property(e => e.Title).HasMaxLength(50);

                entity.HasOne(d => d.Provider)
                    .WithMany(p => p.ProviderContactDetail)
                    .HasForeignKey(d => d.ProviderId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_ProviderContactDetail_Provider");
            });

            modelBuilder.Entity<ProviderPackage>(entity =>
            {
                entity.Property(e => e.AdditionalChargeDataAmount)
                    .HasColumnName("AdditionalCharge_DataAmount")
                    .HasColumnType("decimal(18, 2)");

                entity.Property(e => e.AdditionalChargeDurationAmount)
                    .HasColumnName("AdditionalCharge_DurationAmount")
                    .HasColumnType("decimal(18, 2)");

                entity.Property(e => e.AdditionalChargeMinuteAmount)
                    .HasColumnName("AdditionalCharge_MinuteAmount")
                    .HasColumnType("decimal(18, 2)");

                entity.Property(e => e.AdditionalData).HasColumnName("Additional_Data");

                entity.Property(e => e.AdditionalMinute)
                    .HasColumnName("Additional_Minute")
                    .HasColumnType("decimal(8, 2)");

                entity.Property(e => e.AdditionalMonth).HasColumnName("Additional_Month");

                entity.Property(e => e.CreatedDate)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.CreatedDateInt).HasComputedColumnSql("((datepart(year,[CreatedDate])*(10000)+datepart(month,[CreatedDate])*(100))+datepart(day,[CreatedDate]))");

                entity.Property(e => e.DeviceAmount).HasColumnType("decimal(18, 2)");

                entity.Property(e => e.DevicePenaltyAmount).HasColumnType("decimal(18, 2)");

                entity.Property(e => e.HandsetDetailIds).IsUnicode(false);

                entity.Property(e => e.InGroupMinute).HasColumnType("decimal(8, 2)");

                entity.Property(e => e.InternationalCallMinute).HasColumnType("decimal(8, 2)");

                entity.Property(e => e.LocalMinute).HasColumnType("decimal(8, 2)");

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(80);

                entity.Property(e => e.PackageAmount).HasColumnType("decimal(18, 2)");

                entity.Property(e => e.PackageMinute).HasColumnType("decimal(8, 2)");

                entity.Property(e => e.RoamingMinute).HasColumnType("decimal(8, 2)");

                entity.Property(e => e.StartDate).HasColumnType("date");

                entity.Property(e => e.StartDateInt).HasComputedColumnSql("((datepart(year,[StartDate])*(10000)+datepart(month,[StartDate])*(100))+datepart(day,[StartDate]))");

                entity.Property(e => e.TerminationFees).HasColumnType("decimal(18, 2)");

                entity.Property(e => e.UpdatedDate).HasColumnType("datetime");

                entity.Property(e => e.UpdatedDateInt).HasComputedColumnSql("((datepart(year,[UpdatedDate])*(10000)+datepart(month,[UpdatedDate])*(100))+datepart(day,[UpdatedDate]))");

                entity.HasOne(d => d.InternetDevice)
                    .WithMany(p => p.ProviderPackage)
                    .HasForeignKey(d => d.InternetDeviceId)
                    .HasConstraintName("FK_ProviderPackage_Mst_InternetDeviceDetail");

                entity.HasOne(d => d.Provider)
                    .WithMany(p => p.ProviderPackage)
                    .HasForeignKey(d => d.ProviderId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_ProviderPackage_Provider");

                entity.HasOne(d => d.ServiceType)
                    .WithMany(p => p.ProviderPackage)
                    .HasForeignKey(d => d.ServiceTypeId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_ProviderPackage_Fix_ServiceType");
            });

            modelBuilder.Entity<ProviderService>(entity =>
            {
                entity.HasOne(d => d.Provider)
                    .WithMany(p => p.ProviderService)
                    .HasForeignKey(d => d.ProviderId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_ProviderService_Provider");

                entity.HasOne(d => d.ServiceType)
                    .WithMany(p => p.ProviderService)
                    .HasForeignKey(d => d.ServiceTypeId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_ProviderService_Fix_ServiceType");
            });

            modelBuilder.Entity<RequestTraceLog>(entity =>
            {
                entity.Property(e => e.Browser).HasMaxLength(150);

                entity.Property(e => e.CreatedDate).HasColumnType("datetime");

                entity.Property(e => e.CreatedDateInt).HasComputedColumnSql("((datepart(year,[CreatedDate])*(10000)+datepart(month,[CreatedDate])*(100))+datepart(day,[CreatedDate]))");

                entity.Property(e => e.DeviceId).HasMaxLength(500);

                entity.Property(e => e.Gcmid)
                    .HasColumnName("GCMID")
                    .HasMaxLength(500);

                entity.Property(e => e.Ipaddress)
                    .HasColumnName("IPAddress")
                    .HasMaxLength(50);

                entity.Property(e => e.Os)
                    .HasColumnName("OS")
                    .HasMaxLength(500);

                entity.Property(e => e.PhoneModel).HasMaxLength(500);

                entity.Property(e => e.Version).HasMaxLength(50);
            });

            modelBuilder.Entity<SkypeExcelDetail>(entity =>
            {
                entity.Property(e => e.CallAmount).HasColumnType("decimal(18, 2)");

                entity.Property(e => e.CallDate).HasColumnType("datetime");

                entity.Property(e => e.CallerNumber)
                    .HasMaxLength(20)
                    .IsUnicode(false);

                entity.Property(e => e.ReceiverNumber)
                    .HasMaxLength(20)
                    .IsUnicode(false);

                entity.HasOne(d => d.Currency)
                    .WithMany(p => p.SkypeExcelDetail)
                    .HasForeignKey(d => d.CurrencyId)
                    .HasConstraintName("FK_SkypeExcelDetail_Mst_Currency");

                entity.HasOne(d => d.ExcelUploadLog)
                    .WithMany(p => p.SkypeExcelDetail)
                    .HasForeignKey(d => d.ExcelUploadLogId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_SkypeExcelDetail_ExcelUploadLog");

                entity.HasOne(d => d.ServiceType)
                    .WithMany(p => p.SkypeExcelDetail)
                    .HasForeignKey(d => d.ServiceTypeId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_SkypeExcelDetail_Fix_ServiceType");
            });

            modelBuilder.Entity<TelePhoneNumberAllocationPackage>(entity =>
            {
                entity.Property(e => e.EndDate).HasColumnType("date");

                entity.Property(e => e.EndDateInt).HasComputedColumnSql("((datepart(year,[EndDate])*(10000)+datepart(month,[EndDate])*(100))+datepart(day,[EndDate]))");

                entity.Property(e => e.StartDate).HasColumnType("date");

                entity.Property(e => e.StartDateInt).HasComputedColumnSql("((datepart(year,[StartDate])*(10000)+datepart(month,[StartDate])*(100))+datepart(day,[StartDate]))");

                entity.Property(e => e.UpdatedDate).HasColumnType("datetime");

                entity.Property(e => e.UpdatedDateInt).HasComputedColumnSql("((datepart(year,[UpdatedDate])*(10000)+datepart(month,[UpdatedDate])*(100))+datepart(day,[UpdatedDate]))");

                entity.HasOne(d => d.Package)
                    .WithMany(p => p.TelePhoneNumberAllocationPackage)
                    .HasForeignKey(d => d.PackageId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_TelePhoneNumberAllocationPackage_ProviderPackage");

                entity.HasOne(d => d.TelephoneNumberAllocation)
                    .WithMany(p => p.TelePhoneNumberAllocationPackage)
                    .HasForeignKey(d => d.TelephoneNumberAllocationId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_TelePhoneNumberAllocationPackage_TelephoneNumberAllocation");
            });

            modelBuilder.Entity<TelephoneNumber>(entity =>
            {
                entity.Property(e => e.AccountNumber).HasMaxLength(20);

                entity.Property(e => e.CreatedDate)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.CreatedDateInt).HasComputedColumnSql("((datepart(year,[CreatedDate])*(10000)+datepart(month,[CreatedDate])*(100))+datepart(day,[CreatedDate]))");

                entity.Property(e => e.TelephoneNumber1)
                    .IsRequired()
                    .HasColumnName("TelephoneNumber")
                    .HasMaxLength(20);

                entity.Property(e => e.UpdatedDate)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.UpdatedDateInt).HasComputedColumnSql("((datepart(year,[UpdatedDate])*(10000)+datepart(month,[UpdatedDate])*(100))+datepart(day,[UpdatedDate]))");

                entity.HasOne(d => d.LineType)
                    .WithMany(p => p.TelephoneNumber)
                    .HasForeignKey(d => d.LineTypeId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_TelephoneNumber_Fix_LineType");

                entity.HasOne(d => d.Provider)
                    .WithMany(p => p.TelephoneNumber)
                    .HasForeignKey(d => d.ProviderId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_TelephoneNumber_Provider");
            });

            modelBuilder.Entity<TelephoneNumberAllocation>(entity =>
            {
                entity.Property(e => e.CreatedDate)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.CreatedDateInt).HasComputedColumnSql("((datepart(year,[CreatedDate])*(10000)+datepart(month,[CreatedDate])*(100))+datepart(day,[CreatedDate]))");

                entity.Property(e => e.EmpPfnumber)
                    .IsRequired()
                    .HasColumnName("EmpPFNumber")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.TelephoneNumber)
                    .IsRequired()
                    .HasMaxLength(20);

                entity.Property(e => e.UpdatedDate)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.UpdatedDateInt).HasComputedColumnSql("((datepart(year,[UpdatedDate])*(10000)+datepart(month,[UpdatedDate])*(100))+datepart(day,[UpdatedDate]))");

                entity.HasOne(d => d.AssignType)
                    .WithMany(p => p.TelephoneNumberAllocation)
                    .HasForeignKey(d => d.AssignTypeId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_TelephoneNumberAllocation_Fix_AssignType");

                entity.HasOne(d => d.Employee)
                    .WithMany(p => p.TelephoneNumberAllocation)
                    .HasForeignKey(d => d.EmployeeId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_TelephoneNumberAllocation_Mst_Employee");

                entity.HasOne(d => d.LineStatus)
                    .WithMany(p => p.TelephoneNumberAllocation)
                    .HasForeignKey(d => d.LineStatusId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_TelephoneNumberAllocation_Fix_LineStatus");

                entity.HasOne(d => d.TelephoneNumberNavigation)
                    .WithMany(p => p.TelephoneNumberAllocation)
                    .HasForeignKey(d => d.TelephoneNumberId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_TelephoneNumberAllocation_TelephoneNumber");
            });

            modelBuilder.Entity<TransactionTypeSetting>(entity =>
            {
                entity.Property(e => e.CreatedDate).HasColumnType("datetime");

                entity.Property(e => e.CreatedDateInt).HasComputedColumnSql("((datepart(year,[CreatedDate])*(10000)+datepart(month,[CreatedDate])*(100))+datepart(day,[CreatedDate]))");

                entity.Property(e => e.SetTypeAs).HasColumnName("SetTypeAS");

                entity.Property(e => e.TransactionType)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.Property(e => e.UpdatedDate).HasColumnType("datetime");

                entity.Property(e => e.UpdatedDateInt).HasComputedColumnSql("((datepart(year,[UpdatedDate])*(10000)+datepart(month,[UpdatedDate])*(100))+datepart(day,[UpdatedDate]))");

                entity.HasOne(d => d.Provider)
                    .WithMany(p => p.TransactionTypeSetting)
                    .HasForeignKey(d => d.ProviderId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_TransactionTypeSetting_Provider");

                entity.HasOne(d => d.SetTypeAsNavigation)
                    .WithMany(p => p.TransactionTypeSetting)
                    .HasForeignKey(d => d.SetTypeAs)
                    .HasConstraintName("FK_TransactionTypeSetting_Fix_CallType");
            });
        }
    }
}
