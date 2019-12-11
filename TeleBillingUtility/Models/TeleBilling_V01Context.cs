using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace TeleBillingUtility.Models
{
    public partial class telebilling_v01Context : DbContext
    {  
        public telebilling_v01Context()
        {
        }

        public telebilling_v01Context(DbContextOptions<telebilling_v01Context> options)
            : base(options)
        {
        }

		public virtual DbSet<Auditactionlog> Auditactionlog { get; set; }
		public virtual DbSet<Billdelegate> Billdelegate { get; set; }
        public virtual DbSet<Billdetails> Billdetails { get; set; }
        public virtual DbSet<Billmaster> Billmaster { get; set; }
        public virtual DbSet<BillmasterServicetype> BillmasterServicetype { get; set; }
        public virtual DbSet<Billreimburse> Billreimburse { get; set; }
        public virtual DbSet<Configuration> Configuration { get; set; }
        public virtual DbSet<Emailreminderlog> Emailreminderlog { get; set; }
        public virtual DbSet<Emailtemplate> Emailtemplate { get; set; }
        public virtual DbSet<Employeebillmaster> Employeebillmaster { get; set; }
        public virtual DbSet<Employeebillservicepackage> Employeebillservicepackage { get; set; }
        public virtual DbSet<Exceldetail> Exceldetail { get; set; }
        public virtual DbSet<Exceldetailpbx> Exceldetailpbx { get; set; }
        public virtual DbSet<Exceluploadlog> Exceluploadlog { get; set; }
        public virtual DbSet<ExceluploadlogServicetype> ExceluploadlogServicetype { get; set; }
        public virtual DbSet<Exceluploadlogpbx> Exceluploadlogpbx { get; set; }
        public virtual DbSet<FixAssigntype> FixAssigntype { get; set; }
		public virtual DbSet<FixAuditlogactiontype> FixAuditlogactiontype { get; set; }
		public virtual DbSet<FixBillemployeestatus> FixBillemployeestatus { get; set; }
        public virtual DbSet<FixBillstatus> FixBillstatus { get; set; }
        public virtual DbSet<FixCalltype> FixCalltype { get; set; }
        public virtual DbSet<FixDevice> FixDevice { get; set; }
        public virtual DbSet<FixEmailtemplatetag> FixEmailtemplatetag { get; set; }
        public virtual DbSet<FixEmailtemplatetype> FixEmailtemplatetype { get; set; }
        public virtual DbSet<FixLinestatus> FixLinestatus { get; set; }
        public virtual DbSet<FixLinetype> FixLinetype { get; set; }
        public virtual DbSet<FixLogtype> FixLogtype { get; set; }
        public virtual DbSet<FixServicetype> FixServicetype { get; set; }
		public virtual DbSet<FixNotificationtype> FixNotificationtype { get; set; }
		public virtual DbSet<LogAudittrial> LogAudittrial { get; set; }
        public virtual DbSet<LogEmail> LogEmail { get; set; }
        public virtual DbSet<Mappingexcel> Mappingexcel { get; set; }
        public virtual DbSet<MappingexcelPbx> MappingexcelPbx { get; set; }
        public virtual DbSet<Mappingexcelcolumn> Mappingexcelcolumn { get; set; }
        public virtual DbSet<MappingexcelcolumnPbx> MappingexcelcolumnPbx { get; set; }
        public virtual DbSet<Mappingservicetypefield> Mappingservicetypefield { get; set; }
        public virtual DbSet<MappingservicetypefieldPbx> MappingservicetypefieldPbx { get; set; }
        public virtual DbSet<Memo> Memo { get; set; }
        public virtual DbSet<Memobills> Memobills { get; set; }
        public virtual DbSet<MstBusinessunit> MstBusinessunit { get; set; }
        public virtual DbSet<MstCostcenter> MstCostcenter { get; set; }
        public virtual DbSet<MstCountry> MstCountry { get; set; }
        public virtual DbSet<MstCurrency> MstCurrency { get; set; }
        public virtual DbSet<MstDepartment> MstDepartment { get; set; }
        public virtual DbSet<MstEmployee> MstEmployee { get; set; }
        public virtual DbSet<MstHandsetdetail> MstHandsetdetail { get; set; }
        public virtual DbSet<MstInternetdevicedetail> MstInternetdevicedetail { get; set; }
        public virtual DbSet<MstLink> MstLink { get; set; }
        public virtual DbSet<MstModule> MstModule { get; set; }
        public virtual DbSet<MstRequestaction> MstRequestaction { get; set; }
        public virtual DbSet<MstRole> MstRole { get; set; }
        public virtual DbSet<MstRolerights> MstRolerights { get; set; }
		public virtual DbSet<Notificationlog> Notificationlog { get; set; }
		public virtual DbSet<Operatorcalllog> Operatorcalllog { get; set; }
        public virtual DbSet<Provider> Provider { get; set; }
        public virtual DbSet<Providercontactdetail> Providercontactdetail { get; set; }
        public virtual DbSet<Providerpackage> Providerpackage { get; set; }
        public virtual DbSet<Providerservice> Providerservice { get; set; }
        public virtual DbSet<Requesttracelog> Requesttracelog { get; set; }
        public virtual DbSet<Skypeexceldetail> Skypeexceldetail { get; set; }
        public virtual DbSet<Telephonenumber> Telephonenumber { get; set; }
        public virtual DbSet<Telephonenumberallocation> Telephonenumberallocation { get; set; }
        public virtual DbSet<Telephonenumberallocationpackage> Telephonenumberallocationpackage { get; set; }
        public virtual DbSet<Transactiontypesetting> Transactiontypesetting { get; set; }


        public virtual DbSet<ExceldetailError> ExceldetailError { get; set; }
        public virtual DbSet<ExceldetailpbxError> ExceldetailpbxError { get; set; }
        public virtual DbSet<SkypeexceldetailError> SkypeexceldetailError { get; set; }



        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
				#warning To protect potentially sensitive information in your connection string, you should move it out of source code. See http://go.microsoft.com/fwlink/?LinkId=723263 for guidance on storing connection strings.
                optionsBuilder.UseMySql("Server=172.16.4.185;Database=telebilling_v01;User ID=aspnet;Password=aspnet;persist security info=True;Connect Timeout=200;Max Pool Size=200;Min Pool Size=5;Pooling=true;Connection Lifetime=300");
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
			modelBuilder.Entity<Auditactionlog>(entity =>
			{
				entity.ToTable("auditactionlog");

				entity.HasIndex(e => e.AuditLogActionType)
					.HasName("FK_AuditActionLog_AuditLogActionType");

				entity.HasIndex(e => e.CreatedBy)
					.HasName("FK_AuditActionLog_LoginUserId");

				entity.Property(e => e.Id).HasColumnType("bigint(20)");

				entity.Property(e => e.AuditLogActionType).HasColumnType("bigint(20)");

				entity.Property(e => e.CreatedBy).HasColumnType("bigint(20)");

				entity.Property(e => e.CreatedDate).HasColumnType("datetime");

				entity.Property(e => e.CreatedDateInt).HasColumnType("bigint(20)");

				entity.Property(e => e.Description)
					.IsRequired()
					.HasColumnType("longtext");

				entity.Property(e => e.ReflectedTableId).HasColumnType("bigint(20)");

				entity.HasOne(d => d.AuditLogActionTypeNavigation)
					.WithMany(p => p.Auditactionlog)
					.HasForeignKey(d => d.AuditLogActionType)
					.OnDelete(DeleteBehavior.ClientSetNull)
					.HasConstraintName("FK_AuditActionLog_AuditLogActionType");

				entity.HasOne(d => d.CreatedByNavigation)
					.WithMany(p => p.Auditactionlog)
					.HasForeignKey(d => d.CreatedBy)
					.OnDelete(DeleteBehavior.ClientSetNull)
					.HasConstraintName("FK_AuditActionLog_LoginUserId");
			});

			modelBuilder.Entity<Billdelegate>(entity =>
            {
                entity.ToTable("billdelegate");

                entity.HasIndex(e => e.DelegateEmployeeId)
                    .HasName("FK_BillDelegate_Mst_EmployeeDelegate");

                entity.HasIndex(e => e.EmployeeId)
                    .HasName("FK_BillDelegate_Mst_Employee");

                entity.Property(e => e.Id).HasColumnType("bigint(20)");

                entity.Property(e => e.AllowBillApproval).HasColumnType("bit(1)");

                entity.Property(e => e.AllowBillIdentification).HasColumnType("bit(1)");

                entity.Property(e => e.CreatedBy).HasColumnType("bigint(20)");

                entity.Property(e => e.CreatedDate).HasColumnType("datetime(3)");

                entity.Property(e => e.CreatedDateInt).HasColumnType("bigint(20)");

                entity.Property(e => e.DelegateEmployeeId).HasColumnType("bigint(20)");

                entity.Property(e => e.EmployeeId).HasColumnType("bigint(20)");

                entity.Property(e => e.IsDelete).HasColumnType("bit(1)");

                entity.Property(e => e.TransactionId).HasColumnType("bigint(20)");

                entity.Property(e => e.UpdatedBy).HasColumnType("bigint(20)");

                entity.Property(e => e.UpdatedDate).HasColumnType("datetime");

                entity.Property(e => e.UpdatedDateInt).HasColumnType("bigint(20)");

                entity.HasOne(d => d.DelegateEmployee)
                    .WithMany(p => p.BilldelegateDelegateEmployee)
                    .HasForeignKey(d => d.DelegateEmployeeId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_BillDelegate_Mst_EmployeeDelegate");

                entity.HasOne(d => d.Employee)
                    .WithMany(p => p.BilldelegateEmployee)
                    .HasForeignKey(d => d.EmployeeId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_BillDelegate_Mst_Employee");
            });

            modelBuilder.Entity<Billdetails>(entity =>
            {
                entity.ToTable("billdetails");

                entity.HasIndex(e => e.BillMasterId)
                    .HasName("FK_BillDetails_BillMaster");

                entity.HasIndex(e => e.CallIdentificationType)
                    .HasName("FK_BillDetails_TransactionTypeSetting");

                entity.HasIndex(e => e.CallTransactionTypeId)
                    .HasName("FK_BillDetails_Fix_CallType");

                entity.HasIndex(e => e.EmployeeBillId)
                    .HasName("FK_BillDetails_EmployeeBillMaster");

                entity.HasIndex(e => e.ServiceTypeId)
                    .HasName("FK_BillDetails_Fix_ServiceType");

                entity.Property(e => e.Id).HasColumnType("bigint(20)");

                entity.Property(e => e.AssignTypeId).HasColumnType("bigint(20)");

                entity.Property(e => e.BillMasterId).HasColumnType("bigint(20)");

                entity.Property(e => e.BusinessUnitId).HasColumnType("bigint(20)");

                entity.Property(e => e.CallAmount).HasColumnType("decimal(20,0)");

                entity.Property(e => e.CallAssignedBy).HasColumnType("bigint(20)");

                entity.Property(e => e.CallAssignedDate).HasColumnType("datetime");

                entity.Property(e => e.CallDate).HasColumnType("datetime");

                entity.Property(e => e.CallDuration).HasColumnType("bigint(20)");

                entity.Property(e => e.CallIdentifedBy).HasColumnType("bigint(20)");

                entity.Property(e => e.CallIdentificationType).HasColumnType("int(11)");

                entity.Property(e => e.CallIdentifiedDate).HasColumnType("datetime");

                entity.Property(e => e.CallIwithInGroup)
                    .HasColumnName("CallIWithInGroup")
                    .HasColumnType("bit(1)");

                entity.Property(e => e.CallTime).HasColumnType("time");

                entity.Property(e => e.CallTransactionTypeId).HasColumnType("bigint(11)");

                entity.Property(e => e.CallerName).HasColumnType("varchar(30)");

                entity.Property(e => e.CallerNumber).HasColumnType("varchar(20)");

                entity.Property(e => e.CreatedBy).HasColumnType("bigint(20)");

                entity.Property(e => e.CreatedDate).HasColumnType("datetime");

                entity.Property(e => e.CreatedDateInt).HasColumnType("bigint(20)");

                entity.Property(e => e.Destination).HasColumnType("varchar(50)");

                entity.Property(e => e.EmployeeBillId).HasColumnType("bigint(20)");

                entity.Property(e => e.EmployeeComment).HasColumnType("varchar(50)");

                entity.Property(e => e.GroupId).HasColumnType("bigint(20)");

                entity.Property(e => e.IsAutoAssigned).HasColumnType("bit(1)");

                entity.Property(e => e.ReceiverName).HasColumnType("varchar(30)");

                entity.Property(e => e.ReceiverNumber).HasColumnType("varchar(20)");

                entity.Property(e => e.ServiceTypeId).HasColumnType("bigint(20)");

                entity.Property(e => e.SubscriptionType).HasColumnType("varchar(50)");

                entity.Property(e => e.TransType).HasColumnType("varchar(50)");

                entity.HasOne(d => d.BillMaster)
                    .WithMany(p => p.Billdetails)
                    .HasForeignKey(d => d.BillMasterId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_BillDetails_BillMaster");

                entity.HasOne(d => d.CallIdentificationTypeNavigation)
                    .WithMany(p => p.Billdetails)
                    .HasForeignKey(d => d.CallIdentificationType)
                    .HasConstraintName("FK_BillDetails_TransactionTypeSetting");

                entity.HasOne(d => d.CallTransactionType)
                    .WithMany(p => p.Billdetails)
                    .HasForeignKey(d => d.CallTransactionTypeId)
                    .HasConstraintName("FK_BillDetails_Fix_CallType");

                entity.HasOne(d => d.EmployeeBill)
                    .WithMany(p => p.Billdetails)
                    .HasForeignKey(d => d.EmployeeBillId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_BillDetails_EmployeeBillMaster");

                entity.HasOne(d => d.ServiceType)
                    .WithMany(p => p.Billdetails)
                    .HasForeignKey(d => d.ServiceTypeId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_BillDetails_Fix_ServiceType");
            });

            modelBuilder.Entity<Billmaster>(entity =>
            {
                entity.ToTable("billmaster");

                entity.HasIndex(e => e.BillAllocatedBy)
                    .HasName("FK_BillMaster_MstEmployee");

                entity.HasIndex(e => e.BillStatusId)
                    .HasName("FK_BillMaster_BillStatus");

                entity.HasIndex(e => e.CurrencyId)
                    .HasName("FK_BillMaster_CurrencyId");

                entity.HasIndex(e => e.ProviderId)
                    .HasName("FK_BillMaster_Provider");

                entity.Property(e => e.Id).HasColumnType("bigint(20)");

                entity.Property(e => e.BillAllocatedBy).HasColumnType("bigint(20)");

                entity.Property(e => e.BillAllocationDate).HasColumnType("datetime");

                entity.Property(e => e.BillAllocationDateInt).HasColumnType("bigint(20)");

                entity.Property(e => e.BillAmount).HasColumnType("decimal(20,0)");

                entity.Property(e => e.BillDueDate).HasColumnType("datetime");

                entity.Property(e => e.BillDueDateInt).HasColumnType("bigint(20)");

                entity.Property(e => e.BillMonth).HasColumnType("int(11)");

                entity.Property(e => e.BillNumber)
                    .IsRequired()
                    .HasColumnType("varchar(50)");

                entity.Property(e => e.BillStatusId).HasColumnType("int(11)");

                entity.Property(e => e.BillYear).HasColumnType("int(1)");

                entity.Property(e => e.CreatedBy).HasColumnType("bigint(20)");

                entity.Property(e => e.CreatedDate).HasColumnType("datetime(3)");

                entity.Property(e => e.CreatedDateInt).HasColumnType("bigint(20)");

                entity.Property(e => e.CurrencyId).HasColumnType("bigint(20)");

                entity.Property(e => e.Description).HasColumnType("longtext");

                entity.Property(e => e.IsBusinessOnly).HasColumnType("bit(1)");

                entity.Property(e => e.IsDelete).HasColumnType("bit(1)");

                entity.Property(e => e.ProviderId).HasColumnType("bigint(20)");

                entity.Property(e => e.TransactionId).HasColumnType("bigint(20)");

                entity.Property(e => e.UpdatedBy).HasColumnType("bigint(20)");

                entity.Property(e => e.UpdatedDate).HasColumnType("datetime");

                entity.Property(e => e.UpdatedDateInt).HasColumnType("bigint(20)");

                entity.HasOne(d => d.BillAllocatedByNavigation)
                    .WithMany(p => p.Billmaster)
                    .HasForeignKey(d => d.BillAllocatedBy)
                    .HasConstraintName("FK_BillMaster_MstEmployee");

                entity.HasOne(d => d.BillStatus)
                    .WithMany(p => p.Billmaster)
                    .HasForeignKey(d => d.BillStatusId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_BillMaster_BillStatus");

                entity.HasOne(d => d.Currency)
                    .WithMany(p => p.Billmaster)
                    .HasForeignKey(d => d.CurrencyId)
                    .HasConstraintName("FK_BillMaster_CurrencyId");

                entity.HasOne(d => d.Provider)
                    .WithMany(p => p.Billmaster)
                    .HasForeignKey(d => d.ProviderId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_BillMaster_Provider");
            });

            modelBuilder.Entity<BillmasterServicetype>(entity =>
            {
                entity.ToTable("billmaster_servicetype");

                entity.HasIndex(e => e.BillMasterId)
                    .HasName("FK_BillMasterService_BillMaster");

                entity.HasIndex(e => e.ServiceTypeId)
                    .HasName("FK_BillMasterService_FixServiceType");

                entity.Property(e => e.Id).HasColumnType("bigint(20)");

                entity.Property(e => e.BillMasterId).HasColumnType("bigint(20)");

                entity.Property(e => e.IsDelete).HasColumnType("bit(1)");

                entity.Property(e => e.ServiceTypeId).HasColumnType("bigint(20)");

                entity.Property(e => e.UpdatedBy).HasColumnType("bigint(20)");

                entity.Property(e => e.UpdatedDate).HasColumnType("datetime");

                entity.Property(e => e.UpdatedDateInt).HasColumnType("bigint(20)");

                entity.HasOne(d => d.BillMaster)
                    .WithMany(p => p.BillmasterServicetype)
                    .HasForeignKey(d => d.BillMasterId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_BillMasterService_BillMaster");

                entity.HasOne(d => d.ServiceType)
                    .WithMany(p => p.BillmasterServicetype)
                    .HasForeignKey(d => d.ServiceTypeId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_BillMasterService_FixServiceType");
            });

            modelBuilder.Entity<Billreimburse>(entity =>
            {
                entity.ToTable("billreimburse");

                entity.HasIndex(e => e.BillMasterId)
                    .HasName("FK_BillReImburse_BillMaster");

                entity.HasIndex(e => e.EmployeeBillId)
                    .HasName("FK_BillReImburse_EmployeeBillMaster");

                entity.Property(e => e.Id).HasColumnType("bigint(20)");

                entity.Property(e => e.ApprovalComment).HasColumnType("longtext");

                entity.Property(e => e.ApprovalDate).HasColumnType("datetime(3)");

                entity.Property(e => e.ApproveDateInt).HasColumnType("bigint(20)");

                entity.Property(e => e.ApprovedBy).HasColumnType("bigint(20)");

                entity.Property(e => e.BillMasterId).HasColumnType("bigint(20)");

                entity.Property(e => e.CreatedBy).HasColumnType("bigint(20)");

                entity.Property(e => e.CreatedDate).HasColumnType("datetime(3)");

                entity.Property(e => e.CreatedDateInt).HasColumnType("bigint(20)");

                entity.Property(e => e.CurrencyId).HasColumnType("bigint(20)");

                entity.Property(e => e.Description)
                    .IsRequired()
                    .HasColumnType("longtext");

                entity.Property(e => e.EmployeeBillId).HasColumnType("bigint(20)");

                entity.Property(e => e.IsApproved).HasColumnType("bit(1)");

                entity.Property(e => e.IsDelete).HasColumnType("bit(1)");

                entity.Property(e => e.ReImbruseAmount).HasColumnType("decimal(18,2)");

                entity.Property(e => e.TransactionId).HasColumnType("bigint(20)");

                entity.HasOne(d => d.BillMaster)
                    .WithMany(p => p.Billreimburse)
                    .HasForeignKey(d => d.BillMasterId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_BillReImburse_BillMaster");

                entity.HasOne(d => d.EmployeeBill)
                    .WithMany(p => p.Billreimburse)
                    .HasForeignKey(d => d.EmployeeBillId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_BillReImburse_EmployeeBillMaster");
            });

            modelBuilder.Entity<Configuration>(entity =>
            {
                entity.ToTable("configuration");

                entity.Property(e => e.Id).HasColumnType("bigint(20)");

                entity.Property(e => e.CreatedBy).HasColumnType("bigint(20)");

                entity.Property(e => e.CreatedDate).HasColumnType("datetime(3)");

                entity.Property(e => e.CreatedDateInt).HasColumnType("bigint(20)");

                entity.Property(e => e.NApprovedByLineManager)
                    .HasColumnName("N_ApprovedByLineManager")
                    .HasColumnType("bit(1)");

                entity.Property(e => e.NBillAllocationToEmployee)
                    .HasColumnName("N_BillAllocationToEmployee")
                    .HasColumnType("bit(1)");

                entity.Property(e => e.NBillDelegatesForIdentification)
                    .HasColumnName("N_BillDelegatesForIdentification")
                    .HasColumnType("bit(1)");

                entity.Property(e => e.NChargeBill)
                    .HasColumnName("N_ChargeBill")
                    .HasColumnType("bit(1)");

                entity.Property(e => e.NDelegatesBillForApproval)
                    .HasColumnName("N_DelegatesBillForApproval")
                    .HasColumnType("bit(1)");

                entity.Property(e => e.NMemoApprovalRejection)
                    .HasColumnName("N_MemoApprovalRejection")
                    .HasColumnType("bit(1)");

                entity.Property(e => e.NNewBillReceiveForApproval)
                    .HasColumnName("N_NewBillReceiveForApproval")
                    .HasColumnType("bit(1)");

                entity.Property(e => e.NRejectedByLineManager)
                    .HasColumnName("N_RejectedByLineManager")
                    .HasColumnType("bit(1)");

                entity.Property(e => e.NSendMemo)
                    .HasColumnName("N_SendMemo")
                    .HasColumnType("bit(1)");

                entity.Property(e => e.REmployeeCallIdentificationInterval)
                    .HasColumnName("R_EmployeeCallIdentification_Interval")
                    .HasColumnType("int(11)");

                entity.Property(e => e.REmployeeCallIdentificationIsActive)
                    .HasColumnName("R_EmployeeCallIdentification_IsActive")
                    .HasColumnType("bit(1)");

                entity.Property(e => e.RLinemanagerApprovalInterval)
                    .HasColumnName("R_LinemanagerApproval_Interval")
                    .HasColumnType("int(11)");

                entity.Property(e => e.RLinemanagerApprovalIsActive)
                    .HasColumnName("R_LinemanagerApproval_IsActive")
                    .HasColumnType("bit(1)");

                entity.Property(e => e.TransactionId).HasColumnType("bigint(20)");

                entity.Property(e => e.UpdatedBy).HasColumnType("bigint(20)");

                entity.Property(e => e.UpdatedDate).HasColumnType("datetime");

                entity.Property(e => e.UpdatedDateInt).HasColumnType("bigint(20)");
            });

            modelBuilder.Entity<Emailreminderlog>(entity =>
            {
                entity.ToTable("emailreminderlog");
				
				entity.HasIndex(e => e.EmployeeBillId)
                    .HasName("FK_EmailReminderLog_EmployeeBillMaster");

                entity.HasIndex(e => e.TemplateId)
                    .HasName("FK_EmailREminder_ETemplate");

                entity.Property(e => e.Id).HasColumnType("bigint(20)");

                entity.Property(e => e.CreatedDate).HasColumnType("datetime(3)");

                entity.Property(e => e.CreatedDateInt).HasColumnType("bigint(20)");

                entity.Property(e => e.EmployeeBillId).HasColumnType("bigint(20)");

                entity.Property(e => e.IsReminderMail).HasColumnType("bit(4)");

                entity.Property(e => e.TemplateId).HasColumnType("bigint(20)");

                entity.HasOne(d => d.EmployeeBill)
                    .WithMany(p => p.Emailreminderlog)
                    .HasForeignKey(d => d.EmployeeBillId)
                    .HasConstraintName("FK_EmailReminderLog_EmployeeBillMaster");

				entity.HasOne(d => d.Template)
                    .WithMany(p => p.Emailreminderlog)
                    .HasForeignKey(d => d.TemplateId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_EmailREminder_ETemplate");
            });

            modelBuilder.Entity<Emailtemplate>(entity =>
            {
                entity.ToTable("emailtemplate");

                entity.HasIndex(e => e.EmailTemplateTypeId)
                    .HasName("FK_Email_EType");

                entity.Property(e => e.Id).HasColumnType("bigint(20)");

                entity.Property(e => e.CreatedBy).HasColumnType("bigint(20)");

                entity.Property(e => e.CreatedDate).HasColumnType("datetime(3)");

                entity.Property(e => e.CreatedDateInt).HasColumnType("bigint(20)");

                entity.Property(e => e.EmailBcc).HasColumnType("varchar(150)");

                entity.Property(e => e.EmailFrom)
                    .IsRequired()
                    .HasColumnType("varchar(150)");

                entity.Property(e => e.EmailTemplateTypeId).HasColumnType("bigint(20)");

                entity.Property(e => e.EmailText)
                    .IsRequired()
                    .HasColumnType("longtext");

                entity.Property(e => e.Subject)
                    .IsRequired()
                    .HasColumnType("varchar(150)");

                entity.Property(e => e.TransactionId).HasColumnType("bigint(20)");

                entity.Property(e => e.UpdatedBy).HasColumnType("bigint(20)");

                entity.Property(e => e.UpdatedDate).HasColumnType("datetime");

                entity.Property(e => e.UpdatedDateInt).HasColumnType("bigint(20)");

                entity.HasOne(d => d.EmailTemplateType)
                    .WithMany(p => p.Emailtemplate)
                    .HasForeignKey(d => d.EmailTemplateTypeId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Email_EType");
            });

			modelBuilder.Entity<Employeebillmaster>(entity =>
			{
				entity.ToTable("employeebillmaster");

				entity.HasIndex(e => e.BillDelegatedEmpId)
					.HasName("FK_EmployeeBillMaster_DelegatedEmpId");

				entity.HasIndex(e => e.BillMasterId)
					.HasName("FK_EmployeeBillMaster_BillMaster");

				entity.HasIndex(e => e.CurrencyId)
					.HasName("FK_EmployeeBillMaster_Mst_Currency");

				entity.HasIndex(e => e.EmpBusinessUnitId)
					.HasName("FK_EmployeeBillMaster_Mst_BusinessUnit");

				entity.HasIndex(e => e.EmployeeBillStatus)
					.HasName("FK_EmployeeBillMaster_Fix_BillEmployeeStatus");

				entity.HasIndex(e => e.EmployeeId)
					.HasName("FK_EmployeeBillMaster_Mst_Employee");

				entity.HasIndex(e => e.LinemanagerId)
					.HasName("FK_EmployeeBillMaster_Mst_Employee1");

				entity.HasIndex(e => e.MobileAssignType)
					.HasName("FK_EmployeeBillMaster_Fix_AssignType");

				entity.HasIndex(e => e.ProviderId)
					.HasName("FK_EmployeeBillMaster_Provider");

				entity.Property(e => e.Id).HasColumnType("bigint(20)");

				entity.Property(e => e.ApprovalComment).HasColumnType("longtext");

				entity.Property(e => e.ApprovalDate).HasColumnType("datetime");

				entity.Property(e => e.ApprovalDateInt).HasColumnType("bigint(20)");

				entity.Property(e => e.ApprovalById).HasColumnType("bigint(20)");

				entity.Property(e => e.BillClosedDate).HasColumnType("datetime(3)");

				entity.Property(e => e.BillClosedDateInt).HasColumnType("bigint(20)");

				entity.Property(e => e.BillDelegatedEmpId).HasColumnType("bigint(20)");

				entity.Property(e => e.BillMasterId).HasColumnType("bigint(20)");

				entity.Property(e => e.BillMonth).HasColumnType("int(11)");

				entity.Property(e => e.BillNumber)
					.IsRequired()
					.HasColumnType("varchar(50)");

				entity.Property(e => e.BillYear).HasColumnType("int(11)");

				entity.Property(e => e.CreatedBy).HasColumnType("bigint(20)");

				entity.Property(e => e.CreatedDate).HasColumnType("datetime(3)");

				entity.Property(e => e.CreatedDateInt).HasColumnType("bigint(20)");

				entity.Property(e => e.CurrencyId).HasColumnType("bigint(20)");

				entity.Property(e => e.Description).HasColumnType("longtext");

				entity.Property(e => e.EmpBusinessUnitId).HasColumnType("bigint(20)");

				entity.Property(e => e.EmployeeBillStatus).HasColumnType("int(11)");

				entity.Property(e => e.EmployeeId).HasColumnType("bigint(20)");

				entity.Property(e => e.IdentificationById).HasColumnType("bigint(20)");

				entity.Property(e => e.IdentificationDate).HasColumnType("datetime(3)");

				entity.Property(e => e.IdentificationDateInt).HasColumnType("bigint(20)");

				entity.Property(e => e.IsApproved).HasColumnType("bit(1)");

				entity.Property(e => e.IsApprovedByDelegate).HasColumnType("bit(1)");

				entity.Property(e => e.IsDelete).HasColumnType("bit(1)");

				entity.Property(e => e.IsIdentificationByDelegate).HasColumnType("bit(1)");

				entity.Property(e => e.IsReIdentificationRequest).HasColumnType("bit(1)");

				entity.Property(e => e.IsReImbursementRequest).HasColumnType("bit(1)");

				entity.Property(e => e.LinemanagerId).HasColumnType("bigint(20)");

				entity.Property(e => e.MobileAssignType).HasColumnType("bigint(20)");

				entity.Property(e => e.PreviousEmployeeBillId).HasColumnType("bigint(20)");

				entity.Property(e => e.ProviderId).HasColumnType("bigint(20)");

				entity.Property(e => e.TelephoneNumber).HasColumnType("varchar(20)");

				entity.Property(e => e.TotalBillAmount).HasColumnType("decimal(20,2)");

				entity.Property(e => e.TransactionId).HasColumnType("bigint(20)");

				entity.Property(e => e.UpdatedBy).HasColumnType("bigint(20)");

				entity.Property(e => e.UpdatedDate).HasColumnType("datetime");

				entity.Property(e => e.UpdatedDateInt).HasColumnType("bigint(20)");

				entity.Property(e => e.BillDelegatedEmpId).HasColumnType("bigint(20)");


				entity.HasOne(d => d.BillDelegatedEmp)
					.WithMany(p => p.EmployeebillmasterBillDelegatedEmp)
					.HasForeignKey(d => d.BillDelegatedEmpId)
					.HasConstraintName("FK_EmployeeBillMaster_DelegatedEmpId");

				entity.HasOne(d => d.BillMaster)
					.WithMany(p => p.Employeebillmaster)
					.HasForeignKey(d => d.BillMasterId)
					.OnDelete(DeleteBehavior.ClientSetNull)
					.HasConstraintName("FK_EmployeeBillMaster_BillMaster");

				entity.HasOne(d => d.Currency)
					.WithMany(p => p.Employeebillmaster)
					.HasForeignKey(d => d.CurrencyId)
					.HasConstraintName("FK_EmployeeBillMaster_Mst_Currency");

				entity.HasOne(d => d.EmpBusinessUnit)
					.WithMany(p => p.Employeebillmaster)
					.HasForeignKey(d => d.EmpBusinessUnitId)
					.HasConstraintName("FK_EmployeeBillMaster_Mst_BusinessUnit");

				entity.HasOne(d => d.EmployeeBillStatusNavigation)
					.WithMany(p => p.Employeebillmaster)
					.HasForeignKey(d => d.EmployeeBillStatus)
					.OnDelete(DeleteBehavior.ClientSetNull)
					.HasConstraintName("FK_EmployeeBillMaster_Fix_BillEmployeeStatus");

				entity.HasOne(d => d.Employee)
					.WithMany(p => p.EmployeebillmasterEmployee)
					.HasForeignKey(d => d.EmployeeId)
					.HasConstraintName("FK_EmployeeBillMaster_Mst_Employee");

				entity.HasOne(d => d.Linemanager)
					.WithMany(p => p.EmployeebillmasterLinemanager)
					.HasForeignKey(d => d.LinemanagerId)
					.HasConstraintName("FK_EmployeeBillMaster_Mst_Employee1");

				entity.HasOne(d => d.MobileAssignTypeNavigation)
					.WithMany(p => p.Employeebillmaster)
					.HasForeignKey(d => d.MobileAssignType)
					.HasConstraintName("FK_EmployeeBillMaster_Fix_AssignType");

				entity.HasOne(d => d.Provider)
					.WithMany(p => p.Employeebillmaster)
					.HasForeignKey(d => d.ProviderId)
					.OnDelete(DeleteBehavior.ClientSetNull)
					.HasConstraintName("FK_EmployeeBillMaster_Provider");
			});

			modelBuilder.Entity<Employeebillservicepackage>(entity =>
            {
                entity.ToTable("employeebillservicepackage");

                entity.HasIndex(e => e.EmployeeBillId)
                    .HasName("FK_EmployeeBillServicePackage_EmployeeBillMaster");

                entity.HasIndex(e => e.PackageId)
                    .HasName("FK_EmployeeBillServicePackage_ProviderPackage");

                entity.HasIndex(e => e.ServiceTypeId)
                    .HasName("FK_EmployeeBillServicePackage_Fix_ServiceType");

                entity.Property(e => e.Id).HasColumnType("bigint(20)");

                entity.Property(e => e.BusinessIdentificationAmount).HasColumnType("decimal(20,0)");

                entity.Property(e => e.BusinessTotalAmount).HasColumnType("decimal(20,0)");

                entity.Property(e => e.DeductionAmount).HasColumnType("decimal(20,0)");

                entity.Property(e => e.EmployeeBillId).HasColumnType("bigint(20)");

                entity.Property(e => e.IsDelete).HasColumnType("bit(1)");

                entity.Property(e => e.PackageId).HasColumnType("bigint(20)");

                entity.Property(e => e.PersonalIdentificationAmount).HasColumnType("decimal(20,0)");

                entity.Property(e => e.ServiceTypeId).HasColumnType("bigint(20)");

                entity.Property(e => e.TransactionId).HasColumnType("bigint(20)");

                entity.Property(e => e.UpdateDate).HasColumnType("datetime");

                entity.Property(e => e.UpdateDateInt).HasColumnType("bigint(20)");

                entity.Property(e => e.UpdatedBy).HasColumnType("bigint(20)");

                entity.HasOne(d => d.EmployeeBill)
                    .WithMany(p => p.Employeebillservicepackage)
                    .HasForeignKey(d => d.EmployeeBillId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_EmployeeBillServicePackage_EmployeeBillMaster");

                entity.HasOne(d => d.Package)
                    .WithMany(p => p.Employeebillservicepackage)
                    .HasForeignKey(d => d.PackageId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_EmployeeBillServicePackage_ProviderPackage");

                entity.HasOne(d => d.ServiceType)
                    .WithMany(p => p.Employeebillservicepackage)
                    .HasForeignKey(d => d.ServiceTypeId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_EmployeeBillServicePackage_Fix_ServiceType");
            });

            modelBuilder.Entity<Exceldetail>(entity =>
            {
                entity.ToTable("exceldetail");

                entity.HasIndex(e => e.AssignType)
                    .HasName("FK_ExcelDetail_FXAssignType");

                entity.HasIndex(e => e.BusinessUnitId)
                    .HasName("FK_ExcelDetail_MstBusinessUnit");

                entity.HasIndex(e => e.CallTransactionTypeId)
                    .HasName("FK_EcelDetail_TransactionTypeSetting");

                entity.HasIndex(e => e.CostCenterId)
                    .HasName("FK_ExcelDetail_CostCenter");

                entity.HasIndex(e => e.CurrencyId)
                    .HasName("FK_ExcelDetail_MstCurrecy");

                entity.HasIndex(e => e.EmployeeId)
                    .HasName("FK_ExcelDetail_MstEmployee");

                entity.HasIndex(e => e.ServiceTypeId)
                    .HasName("FK_ExcelDetail_FixServiceType");

                entity.Property(e => e.Id).HasColumnType("bigint(20)");

                entity.Property(e => e.AssignType).HasColumnType("bigint(20)");

                entity.Property(e => e.Bandwidth).HasColumnType("varchar(50)");

                entity.Property(e => e.BusinessUnit).HasColumnType("varchar(200)");

                entity.Property(e => e.BusinessUnitId).HasColumnType("bigint(20)");

                entity.Property(e => e.CallAmount).HasColumnType("decimal(20,0)");

                entity.Property(e => e.CallDate).HasColumnType("datetime");

                entity.Property(e => e.CallDuration).HasColumnType("bigint(20)");

                entity.Property(e => e.CallTransactionTypeId).HasColumnType("bigint(20)");

                entity.Property(e => e.CallWithinGroup).HasColumnType("bit(1)");

                entity.Property(e => e.CallerName).HasColumnType("varchar(150)");

                entity.Property(e => e.CallerNumber).HasColumnType("varchar(20)");

                entity.Property(e => e.CommentOnBandwidth).HasColumnType("longtext");

                entity.Property(e => e.CommentOnPrice).HasColumnType("longtext");

                entity.Property(e => e.CostCenterId).HasColumnType("bigint(20)");

                entity.Property(e => e.CostCentre).HasColumnType("varchar(200)");

                entity.Property(e => e.CurrencyId).HasColumnType("bigint(20)");

                entity.Property(e => e.Description).HasColumnType("longtext");

                entity.Property(e => e.Destination).HasColumnType("longtext");

                entity.Property(e => e.EmployeeId).HasColumnType("bigint(20)");

                entity.Property(e => e.ExcelUploadLogId).HasColumnType("bigint(20)");

                entity.Property(e => e.FinalAnnualChargesKd)
                    .HasColumnName("FinalAnnualChargesKD")
                    .HasColumnType("decimal(20,0)");

                entity.Property(e => e.GroupDetail).HasColumnType("varchar(50)");

                entity.Property(e => e.GroupId).HasColumnType("bigint(20)");

                entity.Property(e => e.InitialDiscountedAnnualPriceKd)
                    .HasColumnName("InitialDiscountedAnnualPriceKD")
                    .HasColumnType("decimal(20,0)");

                entity.Property(e => e.InitialDiscountedMonthlyPriceKd)
                    .HasColumnName("InitialDiscountedMonthlyPriceKD")
                    .HasColumnType("decimal(20,0)");

                entity.Property(e => e.InitialDiscountedSavingMonthlyKd)
                    .HasColumnName("InitialDiscountedSavingMonthlyKD")
                    .HasColumnType("decimal(20,0)");

                entity.Property(e => e.InitialDiscountedSavingYearlyKd)
                    .HasColumnName("InitialDiscountedSavingYearlyKD")
                    .HasColumnType("decimal(20,0)");

                entity.Property(e => e.IsAssigned).HasColumnType("bit(1)");

                entity.Property(e => e.MonthlyPrice).HasColumnType("decimal(21,0)");

                entity.Property(e => e.ReceiverName).HasColumnType("varchar(150)");

                entity.Property(e => e.ReceiverNumber).HasColumnType("varchar(20)");

                entity.Property(e => e.ServiceDetail).HasColumnType("varchar(200)");

                entity.Property(e => e.ServiceTypeId).HasColumnType("bigint(20)");

                entity.Property(e => e.SiteName).HasColumnType("varchar(150)");

                entity.Property(e => e.SubscriptionType).HasColumnType("varchar(50)");

                entity.Property(e => e.TransType).HasColumnType("longtext");

                entity.HasOne(d => d.AssignTypeNavigation)
                    .WithMany(p => p.Exceldetail)
                    .HasForeignKey(d => d.AssignType)
                    .HasConstraintName("FK_ExcelDetail_Fix_AssignType");

                entity.HasOne(d => d.BusinessUnitNavigation)
                    .WithMany(p => p.Exceldetail)
                    .HasForeignKey(d => d.BusinessUnitId)
                    .HasConstraintName("FK_ExcelDetail_MstBusinessUnit");

                entity.HasOne(d => d.CallTransactionType)
                    .WithMany(p => p.Exceldetail)
                    .HasForeignKey(d => d.CallTransactionTypeId)
                    .HasConstraintName("FK_EcelDetail_TransactionTypeSetting");

                entity.HasOne(d => d.CostCenter)
                    .WithMany(p => p.Exceldetail)
                    .HasForeignKey(d => d.CostCenterId)
                    .HasConstraintName("FK_ExcelDetail_CostCenter");

                entity.HasOne(d => d.Currency)
                    .WithMany(p => p.Exceldetail)
                    .HasForeignKey(d => d.CurrencyId)
                    .HasConstraintName("FK_ExcelDetail_MstCurrecy");

                entity.HasOne(d => d.Employee)
                    .WithMany(p => p.Exceldetail)
                    .HasForeignKey(d => d.EmployeeId)
                    .HasConstraintName("FK_ExcelDetail_MstEmployee");

                entity.HasOne(d => d.ServiceType)
                    .WithMany(p => p.Exceldetail)
                    .HasForeignKey(d => d.ServiceTypeId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_ExcelDetail_FixServiceType");
            });

            modelBuilder.Entity<Exceldetailpbx>(entity =>
            {
                entity.ToTable("exceldetailpbx");

                entity.HasIndex(e => e.ExcelUploadLogId)
                    .HasName("FK_ExcelDetailPBX_ExcelUploadPBX");

                entity.Property(e => e.Id).HasColumnType("bigint(20)");

                entity.Property(e => e.Band).HasColumnType("varchar(50)");

                entity.Property(e => e.CallAmount).HasColumnType("decimal(20,0)");

                entity.Property(e => e.CallDate).HasColumnType("datetime");

                entity.Property(e => e.CallDuration).HasColumnType("bigint(20)");

                entity.Property(e => e.CallTime).HasColumnType("time");

                entity.Property(e => e.CallType).HasColumnType("varchar(50)");

                entity.Property(e => e.ClassificationCode).HasColumnType("varchar(50)");

                entity.Property(e => e.CodeNumber).HasColumnType("varchar(50)");

                entity.Property(e => e.ConnectingParty).HasColumnType("varchar(50)");

                entity.Property(e => e.CurrencyId).HasColumnType("bigint(20)");

                entity.Property(e => e.DestinationType).HasColumnType("varchar(50)");

                entity.Property(e => e.DistantNumber).HasColumnType("varchar(50)");

                entity.Property(e => e.ExcelUploadLogId).HasColumnType("bigint(20)");

                entity.Property(e => e.IsMatched).HasColumnType("bit(1)");

                entity.Property(e => e.Name1).HasColumnType("varchar(50)");

                entity.Property(e => e.Name2).HasColumnType("varchar(50)");

                entity.Property(e => e.Name3).HasColumnType("varchar(50)");

                entity.Property(e => e.Name4).HasColumnType("varchar(50)");

                entity.Property(e => e.OtherParty).HasColumnType("varchar(50)");

                entity.Property(e => e.Place).HasColumnType("varchar(50)");

                entity.Property(e => e.Rate).HasColumnType("varchar(50)");

                entity.Property(e => e.RingingTime).HasColumnType("int(11)");

                entity.Property(e => e.SkypeMatchedId).HasColumnType("bigint(20)");

                entity.HasOne(d => d.ExcelUploadLog)
                    .WithMany(p => p.Exceldetailpbx)
                    .HasForeignKey(d => d.ExcelUploadLogId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_ExcelDetailPBX_ExcelUploadPBX");
            });

            modelBuilder.Entity<Exceluploadlog>(entity =>
            {
                entity.ToTable("exceluploadlog");

                entity.HasIndex(e => e.DeviceId)
                    .HasName("FK_ExcelUploadLogPbx_Fix_Device");

                entity.HasIndex(e => e.ProviderId)
                    .HasName("FK_ExcelUploadLog_Provider");

                entity.Property(e => e.Id).HasColumnType("bigint(20)");

                entity.Property(e => e.CreatedBy).HasColumnType("bigint(20)");

                entity.Property(e => e.CreatedDate).HasColumnType("datetime");

                entity.Property(e => e.CreatedDateInt).HasColumnType("bigint(20)");

                entity.Property(e => e.DeviceId).HasColumnType("bigint(11)");

                entity.Property(e => e.ExcelFileName)
                    .IsRequired()
                    .HasColumnType("longtext");

                entity.Property(e => e.FileNameGuid)
                    .IsRequired()
                    .HasColumnType("longtext");

                entity.Property(e => e.IsDelete).HasColumnType("bit(1)");

                entity.Property(e => e.IsPbxupload)
                    .HasColumnName("IsPBXUpload")
                    .HasColumnType("bit(1)");

                entity.Property(e => e.Month).HasColumnType("int(11)");

                entity.Property(e => e.ProviderId).HasColumnType("bigint(20)");

                entity.Property(e => e.TotalImportedBillAmount).HasColumnType("decimal(20,0)");

                entity.Property(e => e.TotalRecordImportCount).HasColumnType("int(11)");

                entity.Property(e => e.TransactionId).HasColumnType("bigint(20)");

                entity.Property(e => e.UpdatedBy).HasColumnType("bigint(20)");

                entity.Property(e => e.UpdatedDate).HasColumnType("datetime");

                entity.Property(e => e.UpdatedDateInt).HasColumnType("bigint(20)");

                entity.Property(e => e.UploadBy).HasColumnType("bigint(20)");

                entity.Property(e => e.UploadDate).HasColumnType("datetime");

                entity.Property(e => e.UploadDateInt).HasColumnType("bigint(20)");

                entity.Property(e => e.Year).HasColumnType("int(11)");

                entity.HasOne(d => d.Device)
                    .WithMany(p => p.Exceluploadlog)
                    .HasForeignKey(d => d.DeviceId)
                    .HasConstraintName("FK_ExcelUploadLogPbx_Fix_Device");

                entity.HasOne(d => d.Provider)
                    .WithMany(p => p.Exceluploadlog)
                    .HasForeignKey(d => d.ProviderId)
                    .HasConstraintName("FK_ExcelUploadLog_Provider");
            });

            modelBuilder.Entity<ExceluploadlogServicetype>(entity =>
            {
                entity.ToTable("exceluploadlog_servicetype");

                entity.HasIndex(e => e.ExcelUploadLogId)
                    .HasName("FK_ExcelUploadLogService_UploadLog");

                entity.HasIndex(e => e.ServiceTypeId)
                    .HasName("FK_ExcelUploadLog_Service");

                entity.Property(e => e.Id).HasColumnType("bigint(20)");

                entity.Property(e => e.ExcelUploadLogId).HasColumnType("bigint(20)");

                entity.Property(e => e.IsAllocated).HasColumnType("bit(1)");

                entity.Property(e => e.IsDelete).HasColumnType("bit(1)");

                entity.Property(e => e.ServiceTypeId).HasColumnType("bigint(20)");

                entity.Property(e => e.TransactionId).HasColumnType("bigint(20)");

                entity.Property(e => e.UpdatedBy).HasColumnType("bigint(20)");

                entity.Property(e => e.UpdatedDate).HasColumnType("datetime");

                entity.Property(e => e.UpdatedDateInt).HasColumnType("bigint(20)");

                entity.HasOne(d => d.ExcelUploadLog)
                    .WithMany(p => p.ExceluploadlogServicetype)
                    .HasForeignKey(d => d.ExcelUploadLogId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_ExcelUploadLogService_UploadLog");

                entity.HasOne(d => d.ServiceType)
                    .WithMany(p => p.ExceluploadlogServicetype)
                    .HasForeignKey(d => d.ServiceTypeId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_ExcelUploadLog_Service");
            });

            modelBuilder.Entity<Exceluploadlogpbx>(entity =>
            {
                entity.ToTable("exceluploadlogpbx");

                entity.HasIndex(e => e.DeviceId)
                    .HasName("FK_ExcelUploadLogPbx_Device");

                entity.Property(e => e.Id).HasColumnType("bigint(20)");

                entity.Property(e => e.CreatedBy).HasColumnType("bigint(20)");

                entity.Property(e => e.CreatedDate).HasColumnType("datetime");

                entity.Property(e => e.CreatedDateInt).HasColumnType("bigint(20)");

                entity.Property(e => e.DeviceId).HasColumnType("bigint(20)");

                entity.Property(e => e.ExcelFileName)
                    .IsRequired()
                    .HasColumnType("longtext");

                entity.Property(e => e.FileNameGuid)
                    .IsRequired()
                    .HasColumnType("longtext");

                entity.Property(e => e.IsDelete).HasColumnType("bit(1)");

                entity.Property(e => e.Month).HasColumnType("int(11)");

                entity.Property(e => e.ProviderId).HasColumnType("bigint(20)");

                entity.Property(e => e.TotalImportedBillAmount).HasColumnType("decimal(20,0)");

                entity.Property(e => e.TotalRecordImportCount).HasColumnType("int(11)");

                entity.Property(e => e.TransactionId).HasColumnType("bigint(20)");

                entity.Property(e => e.UpdatedBy).HasColumnType("bigint(20)");

                entity.Property(e => e.UpdatedDate).HasColumnType("datetime");

                entity.Property(e => e.UpdatedDateInt).HasColumnType("bigint(20)");

                entity.Property(e => e.UploadBy).HasColumnType("bigint(20)");

                entity.Property(e => e.UploadDate).HasColumnType("datetime");

                entity.Property(e => e.UploadDateInt).HasColumnType("bigint(20)");

                entity.Property(e => e.Year).HasColumnType("int(11)");

                entity.HasOne(d => d.Device)
                    .WithMany(p => p.Exceluploadlogpbx)
                    .HasForeignKey(d => d.DeviceId)
                    .HasConstraintName("FK_ExcelUploadLogPbx_Device");
            });

            modelBuilder.Entity<FixAssigntype>(entity =>
            {
                entity.ToTable("fix_assigntype");

                entity.Property(e => e.Id).HasColumnType("bigint(20)");

                entity.Property(e => e.CreatedBy).HasColumnType("bigint(20)");

                entity.Property(e => e.CreatedDate).HasColumnType("datetime");

                entity.Property(e => e.IsActive).HasColumnType("bit(1)");

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasColumnType("varchar(50)");
            });

			modelBuilder.Entity<FixAuditlogactiontype>(entity =>
			{
				entity.ToTable("fix_auditlogactiontype");

				entity.Property(e => e.Id).HasColumnType("bigint(20)");

				entity.Property(e => e.IsDeleted).HasColumnType("bit(1)");

				entity.Property(e => e.Name)
					.IsRequired()
					.HasColumnType("varchar(255)");
			});

			modelBuilder.Entity<FixBillemployeestatus>(entity =>
            {
                entity.ToTable("fix_billemployeestatus");

                entity.Property(e => e.Id).HasColumnType("int(11)");

                entity.Property(e => e.CreatedBy).HasColumnType("bigint(20)");

                entity.Property(e => e.CreatedDate).HasColumnType("datetime");

                entity.Property(e => e.IsActive).HasColumnType("bit(1)");

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasColumnType("varchar(50)");
            });

            modelBuilder.Entity<FixBillstatus>(entity =>
            {
                entity.ToTable("fix_billstatus");

                entity.Property(e => e.Id).HasColumnType("int(11)");

                entity.Property(e => e.CreatedBy).HasColumnType("bigint(20)");

                entity.Property(e => e.CreatedDate).HasColumnType("datetime");

                entity.Property(e => e.IsActive).HasColumnType("bit(1)");

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasColumnType("varchar(50)");
            });

            modelBuilder.Entity<FixCalltype>(entity =>
            {
                entity.ToTable("fix_calltype");

                entity.Property(e => e.Id).HasColumnType("int(11)");

                entity.Property(e => e.CreatedBy).HasColumnType("bigint(20)");

                entity.Property(e => e.CreatedDate).HasColumnType("datetime");

                entity.Property(e => e.IsActive).HasColumnType("bit(1)");

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasColumnType("varchar(50)");
            });

            modelBuilder.Entity<FixDevice>(entity =>
            {
                entity.ToTable("fix_device");

                entity.Property(e => e.Id).HasColumnType("bigint(20)");

                entity.Property(e => e.CreatedBy).HasColumnType("bigint(20)");

                entity.Property(e => e.CreatedDate).HasColumnType("datetime");

                entity.Property(e => e.IsActive).HasColumnType("bit(1)");

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasColumnType("varchar(50)");
            });

            modelBuilder.Entity<FixEmailtemplatetag>(entity =>
            {
                entity.ToTable("fix_emailtemplatetag");

                entity.Property(e => e.Id).HasColumnType("int(11)");

                entity.Property(e => e.CreatedBy).HasColumnType("bigint(20)");

                entity.Property(e => e.CreatedDate).HasColumnType("datetime");

                entity.Property(e => e.IsActive).HasColumnType("bit(1)");

                entity.Property(e => e.TemplateTag)
                    .IsRequired()
                    .HasColumnType("varchar(50)");

                entity.Property(e => e.TemplateText)
                    .IsRequired()
                    .HasColumnType("varchar(50)");
            });

            modelBuilder.Entity<FixEmailtemplatetype>(entity =>
            {
                entity.ToTable("fix_emailtemplatetype");

                entity.Property(e => e.Id).HasColumnType("bigint(20)");

                entity.Property(e => e.CreatedBy).HasColumnType("bigint(20)");

                entity.Property(e => e.CreatedDate).HasColumnType("datetime");

                entity.Property(e => e.IsActive).HasColumnType("bit(1)");

                entity.Property(e => e.TemplateType)
                    .IsRequired()
                    .HasColumnType("varchar(50)");
            });

            modelBuilder.Entity<FixLinestatus>(entity =>
            {
                entity.ToTable("fix_linestatus");

                entity.Property(e => e.Id).HasColumnType("int(11)");

                entity.Property(e => e.CreatedBy).HasColumnType("bigint(20)");

                entity.Property(e => e.CreatedDate).HasColumnType("datetime");

                entity.Property(e => e.IsActive).HasColumnType("bit(1)");

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasColumnType("varchar(50)");
            });

            modelBuilder.Entity<FixLinetype>(entity =>
            {
                entity.ToTable("fix_linetype");

                entity.Property(e => e.Id).HasColumnType("int(11)");

                entity.Property(e => e.CreatedBy).HasColumnType("bigint(20)");

                entity.Property(e => e.CreatedDate).HasColumnType("datetime");

                entity.Property(e => e.IsActive).HasColumnType("bit(1)");

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasColumnType("varchar(50)");
            });

            modelBuilder.Entity<FixLogtype>(entity =>
            {
                entity.HasKey(e => e.LogTypeId)
                    .HasName("PRIMARY");

                entity.ToTable("fix_logtype");

                entity.Property(e => e.LogTypeId).HasColumnType("bigint(20)");

                entity.Property(e => e.LogText)
                    .IsRequired()
                    .HasColumnType("longtext");

                entity.Property(e => e.TypeName)
                    .IsRequired()
                    .HasColumnType("varchar(50)");
            });

			modelBuilder.Entity<FixNotificationtype>(entity =>
			{
				entity.ToTable("fix_notificationtype");

				entity.Property(e => e.Id).HasColumnType("bigint(20)");

				entity.Property(e => e.CreatedDate).HasColumnType("datetime");

				entity.Property(e => e.Name)
					.IsRequired()
					.HasColumnType("varchar(255)");
			});

			modelBuilder.Entity<FixServicetype>(entity =>
            {
                entity.ToTable("fix_servicetype");

                entity.Property(e => e.Id).HasColumnType("bigint(20)");

                entity.Property(e => e.CreatedBy).HasColumnType("bigint(20)");

                entity.Property(e => e.CreatedDate).HasColumnType("datetime");

                entity.Property(e => e.IsActive).HasColumnType("bit(1)");

                entity.Property(e => e.IsBusinessOnly).HasColumnType("bit(1)");

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasColumnType("varchar(50)");
            });

            modelBuilder.Entity<LogAudittrial>(entity =>
            {
                entity.HasKey(e => e.AuditTrialLogId)
                    .HasName("PRIMARY");

                entity.ToTable("log_audittrial");

                entity.HasIndex(e => e.LogTypeId)
                    .HasName("FK_Audit_FixLogType");

                entity.Property(e => e.AuditTrialLogId).HasColumnType("bigint(20)");

                entity.Property(e => e.AuditDate).HasColumnType("datetime(3)");

                entity.Property(e => e.AuditDateInt).HasColumnType("bigint(20)");

                entity.Property(e => e.Browser).HasColumnType("varchar(150)");

                entity.Property(e => e.Description)
                    .IsRequired()
                    .HasColumnType("longtext");

                entity.Property(e => e.Ipaddress)
                    .HasColumnName("IPAddress")
                    .HasColumnType("varchar(50)");

                entity.Property(e => e.IsMobile).HasColumnType("bit(1)");

                entity.Property(e => e.LogTypeId).HasColumnType("bigint(20)");

                entity.Property(e => e.UserId).HasColumnType("bigint(20)");

                entity.Property(e => e.Version).HasColumnType("varchar(50)");

                entity.HasOne(d => d.LogType)
                    .WithMany(p => p.LogAudittrial)
                    .HasForeignKey(d => d.LogTypeId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Audit_FixLogType");
            });

            modelBuilder.Entity<LogEmail>(entity =>
            {
                entity.ToTable("log_email");

                entity.Property(e => e.Id).HasColumnType("bigint(20)");

                entity.Property(e => e.BillId).HasColumnType("bigint(20)");

                entity.Property(e => e.CreatedDate).HasColumnType("datetime(3)");

                entity.Property(e => e.CreatedDateInt).HasColumnType("bigint(20)");

                entity.Property(e => e.EmailBcc)
                    .IsRequired()
                    .HasColumnName("EmailBCC")
                    .HasColumnType("varchar(150)");

                entity.Property(e => e.EmailFrom)
                    .IsRequired()
                    .HasColumnType("varchar(150)");

                entity.Property(e => e.EmailTemplateTypeId).HasColumnType("bigint(20)");

                entity.Property(e => e.EmailText)
                    .IsRequired()
                    .HasColumnType("longtext");

                entity.Property(e => e.EmailTo)
                    .IsRequired()
                    .HasColumnType("varchar(150)");

                entity.Property(e => e.EmailTypeId).HasColumnType("bigint(20)");

                entity.Property(e => e.IsSent).HasColumnType("bit(1)");

                entity.Property(e => e.SendDate).HasColumnType("datetime(3)");

                entity.Property(e => e.SendDateInt).HasColumnType("bigint(20)");

                entity.Property(e => e.Subject)
                    .IsRequired()
                    .HasColumnType("varchar(50)");
            });

            modelBuilder.Entity<Mappingexcel>(entity =>
            {
                entity.ToTable("mappingexcel");

                entity.HasIndex(e => e.ProviderId)
                    .HasName("FK_MP_P");

                entity.HasIndex(e => e.ServiceTypeId)
                    .HasName("FK_MP_ST");

                entity.Property(e => e.Id).HasColumnType("bigint(20)");

                entity.Property(e => e.CreatedBy).HasColumnType("bigint(20)");

                entity.Property(e => e.CreatedDate).HasColumnType("datetime(3)");

                entity.Property(e => e.CreatedDateInt).HasColumnType("bigint(20)");

                entity.Property(e => e.CurrencyId).HasColumnType("bigint(20)");

                entity.Property(e => e.ExcelColumnNameForTitle).HasColumnType("varchar(6)");

                entity.Property(e => e.ExcelReadingColumn)
                    .HasColumnType("varchar(6)");

                entity.Property(e => e.HaveHeader).HasColumnType("bit(1)");

                entity.Property(e => e.HaveTitle).HasColumnType("bit(1)");

                entity.Property(e => e.IsActive).HasColumnType("bit(1)");

                entity.Property(e => e.IsDelete).HasColumnType("bit(1)");

                entity.Property(e => e.ProviderId).HasColumnType("bigint(20)");

                entity.Property(e => e.ServiceTypeId).HasColumnType("bigint(20)");

                entity.Property(e => e.TitleName).HasColumnType("varchar(150)");

                entity.Property(e => e.TransactionId).HasColumnType("bigint(20)");

                entity.Property(e => e.IsCommonMapped).HasColumnType("bit(1)");

                entity.Property(e => e.MappedMappingId).HasColumnType("bigint(20)");

                entity.Property(e => e.MappedServiceTypeId).HasColumnType("bigint(20)");


                entity.Property(e => e.UpdatedBy).HasColumnType("bigint(20)");

                entity.Property(e => e.UpdatedDate).HasColumnType("datetime");

                entity.Property(e => e.UpdatedDateInt).HasColumnType("bigint(20)");

                entity.Property(e => e.WorkSheetNo).HasColumnType("bigint(20)");

                entity.HasOne(d => d.Provider)
                    .WithMany(p => p.Mappingexcel)
                    .HasForeignKey(d => d.ProviderId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_MP_P");

                entity.HasOne(d => d.ServiceType)
                    .WithMany(p => p.Mappingexcel)
                    .HasForeignKey(d => d.ServiceTypeId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_MP_ST");
            });

            modelBuilder.Entity<MappingexcelPbx>(entity =>
            {
                entity.ToTable("mappingexcel_pbx");

                entity.HasIndex(e => e.DeviceId)
                    .HasName("FK_MPBX_D");

                entity.Property(e => e.Id).HasColumnType("bigint(20)");

                entity.Property(e => e.CreatedBy).HasColumnType("bigint(20)");

                entity.Property(e => e.CreatedDate).HasColumnType("datetime(3)");

                entity.Property(e => e.CreatedDateInt).HasColumnType("bigint(20)");

                entity.Property(e => e.CurrencyId).HasColumnType("bigint(20)");

                entity.Property(e => e.DeviceId).HasColumnType("bigint(20)");

                entity.Property(e => e.ExcelColumnNameForTitle).HasColumnType("varchar(6)");

				entity.Property(e => e.ExcelReadingColumn).HasColumnType("varchar(6)");
				
                entity.Property(e => e.HaveHeader).HasColumnType("bit(1)");

                entity.Property(e => e.HaveTitle).HasColumnType("bit(1)");

                entity.Property(e => e.IsActive).HasColumnType("bit(1)");

                entity.Property(e => e.IsDelete).HasColumnType("bit(1)");

                entity.Property(e => e.TitleName).HasColumnType("varchar(150)");

                entity.Property(e => e.TransactionId).HasColumnType("bigint(20)");

                entity.Property(e => e.UpdatedBy).HasColumnType("bigint(20)");

                entity.Property(e => e.UpdatedDate).HasColumnType("datetime");

                entity.Property(e => e.UpdatedDateInt).HasColumnType("bigint(20)");

                entity.Property(e => e.WorkSheetNo).HasColumnType("bigint(20)");

                entity.HasOne(d => d.Device)
                    .WithMany(p => p.MappingexcelPbx)
                    .HasForeignKey(d => d.DeviceId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_MPBX_D");
            });

            modelBuilder.Entity<Mappingexcelcolumn>(entity =>
            {
                entity.ToTable("mappingexcelcolumn");

                entity.HasIndex(e => e.MappingExcelId)
                    .HasName("FK_MappingExcelColumn_MappingExcel");

                entity.HasIndex(e => e.MappingServiceTypeFieldId)
                    .HasName("FK_MEXC_FST");

                entity.Property(e => e.Id).HasColumnType("bigint(20)");

                entity.Property(e => e.ExcelcolumnName)
                    .IsRequired()
                    .HasColumnType("varchar(10)");

                entity.Property(e => e.FormatField).HasColumnType("varchar(100)");

                entity.Property(e => e.MappingExcelId).HasColumnType("bigint(20)");

                entity.Property(e => e.MappingServiceTypeFieldId).HasColumnType("bigint(20)");

                entity.HasOne(d => d.MappingExcel)
                    .WithMany(p => p.Mappingexcelcolumn)
                    .HasForeignKey(d => d.MappingExcelId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_MappingExcelColumn_MappingExcel");

                entity.HasOne(d => d.MappingServiceTypeField)
                    .WithMany(p => p.Mappingexcelcolumn)
                    .HasForeignKey(d => d.MappingServiceTypeFieldId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_MappingExcelColumn_MappingServiceTypeField");
            });

            modelBuilder.Entity<MappingexcelcolumnPbx>(entity =>
            {
                entity.ToTable("mappingexcelcolumn_pbx");

                entity.HasIndex(e => e.MappingExcelId)
                    .HasName("FK_MappingExcelColumn_PBX_MappingExcel_PBX");

                entity.HasIndex(e => e.MappingServiceTypeFieldId)
                    .HasName("FK_MPBXEXC_FST");

                entity.Property(e => e.Id).HasColumnType("bigint(20)");

                entity.Property(e => e.ExcelcolumnName)
                    .IsRequired()
                    .HasColumnType("varchar(10)");

                entity.Property(e => e.FormatField).HasColumnType("varchar(100)");

                entity.Property(e => e.MappingExcelId).HasColumnType("bigint(20)");

                entity.Property(e => e.MappingServiceTypeFieldId).HasColumnType("bigint(20)");

                entity.HasOne(d => d.MappingExcel)
                    .WithMany(p => p.MappingexcelcolumnPbx)
                    .HasForeignKey(d => d.MappingExcelId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_MappingExcelColumn_PBX_MappingExcel_PBX");

                entity.HasOne(d => d.MappingServiceTypeField)
                    .WithMany(p => p.MappingexcelcolumnPbx)
                    .HasForeignKey(d => d.MappingServiceTypeFieldId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_MappingExcelColumn_PBX_MappingServiceTypeField_PBX");
            });

            modelBuilder.Entity<Mappingservicetypefield>(entity =>
            {
                entity.ToTable("mappingservicetypefield");

                entity.HasIndex(e => e.ServiceTypeId)
                    .HasName("FK_MSTF_FST");

                entity.Property(e => e.Id).HasColumnType("bigint(20)");

                entity.Property(e => e.DbcolumnName)
                    .IsRequired()
                    .HasColumnName("DBColumnName")
                    .HasColumnType("varchar(60)");

                entity.Property(e => e.DbtableName)
                    .IsRequired()
                    .HasColumnName("DBTableName")
                    .HasColumnType("varchar(60)");

                entity.Property(e => e.DisplayFieldName)
                    .IsRequired()
                    .HasColumnType("varchar(60)");

                entity.Property(e => e.DisplayOrder).HasColumnType("int(11)");

                entity.Property(e => e.IsRequired).HasColumnType("bit(1)");

                entity.Property(e => e.IsSpecial).HasColumnType("bit(1)");

                entity.Property(e => e.ServiceTypeId).HasColumnType("bigint(20)");

                entity.HasOne(d => d.ServiceType)
                    .WithMany(p => p.Mappingservicetypefield)
                    .HasForeignKey(d => d.ServiceTypeId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_MSTF_FST");
            });

            modelBuilder.Entity<MappingservicetypefieldPbx>(entity =>
            {
                entity.ToTable("mappingservicetypefield_pbx");

                entity.HasIndex(e => e.DeviceId)
                    .HasName("FK_MSTFPBX_FD");

                entity.Property(e => e.Id).HasColumnType("bigint(20)");

                entity.Property(e => e.DbcolumnName)
                    .IsRequired()
                    .HasColumnName("DBColumnName")
                    .HasColumnType("varchar(60)");

                entity.Property(e => e.DbtableName)
                    .IsRequired()
                    .HasColumnName("DBTableName")
                    .HasColumnType("varchar(60)");

                entity.Property(e => e.DeviceId).HasColumnType("bigint(20)");

                entity.Property(e => e.DisplayFieldName)
                    .IsRequired()
                    .HasColumnType("varchar(60)");

                entity.Property(e => e.DisplayOrder).HasColumnType("int(11)");

                entity.Property(e => e.IsRequired).HasColumnType("bit(1)");

                entity.Property(e => e.IsSpecial).HasColumnType("bit(1)");

                entity.HasOne(d => d.Device)
                    .WithMany(p => p.MappingservicetypefieldPbx)
                    .HasForeignKey(d => d.DeviceId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_MSTFPBX_FD");
            });

            modelBuilder.Entity<Memo>(entity =>
            {
                entity.ToTable("memo");

                entity.HasIndex(e => e.ProviderId)
                    .HasName("FK_Memo_P");

                entity.Property(e => e.Id).HasColumnType("bigint(20)");

                entity.Property(e => e.ApprovedBy).HasColumnType("bigint(20)");

                entity.Property(e => e.ApprovedDate).HasColumnType("datetime(3)");

                entity.Property(e => e.ApprovedDateInt).HasColumnType("bigint(20)");

                entity.Property(e => e.Bank).HasColumnType("varchar(100)");

                entity.Property(e => e.Comment).HasColumnType("longtext");

                entity.Property(e => e.CreatedBy).HasColumnType("bigint(20)");

                entity.Property(e => e.CreatedDate).HasColumnType("datetime(3)");

                entity.Property(e => e.CreatedDateInt).HasColumnType("bigint(20)");

                entity.Property(e => e.Date).HasColumnType("datetime(3)");

                entity.Property(e => e.Ibancode)
                    .HasColumnName("IBANCode")
                    .HasColumnType("varchar(100)");

                entity.Property(e => e.IsApproved).HasColumnType("bit(1)");

                entity.Property(e => e.IsBankTransaction).HasColumnType("bit(1)");

                entity.Property(e => e.IsDelete).HasColumnType("bit(1)");

                entity.Property(e => e.Month).HasColumnType("int(11)");

                entity.Property(e => e.ProviderId).HasColumnType("bigint(20)");

                entity.Property(e => e.RefrenceNo)
                    .IsRequired()
                    .HasColumnType("varchar(200)");

                entity.Property(e => e.Subject)
                    .IsRequired()
                    .HasColumnType("varchar(50)");

                entity.Property(e => e.Swiftcode)
                    .HasColumnName("SWIFTCode")
                    .HasColumnType("varchar(100)");

                entity.Property(e => e.TotalAmount).HasColumnType("decimal(18,0)");

                entity.Property(e => e.TransactionId).HasColumnType("bigint(20)");

                entity.Property(e => e.UpdatedBy).HasColumnType("bigint(20)");

                entity.Property(e => e.UpdatedDate).HasColumnType("datetime");

                entity.Property(e => e.UpdatedDateInt).HasColumnType("bigint(20)");

                entity.Property(e => e.Year).HasColumnType("int(11)");

                entity.HasOne(d => d.Provider)
                    .WithMany(p => p.Memo)
                    .HasForeignKey(d => d.ProviderId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Memo_P");
            });

            modelBuilder.Entity<Memobills>(entity =>
            {
                entity.ToTable("memobills");

                entity.HasIndex(e => e.BillId)
                    .HasName("FK_MemoBills_BillMaster");

                entity.HasIndex(e => e.MemoId)
                    .HasName("FK_MemoBill_Momo");

                entity.Property(e => e.Id).HasColumnType("bigint(20)");

                entity.Property(e => e.BillId).HasColumnType("bigint(20)");

                entity.Property(e => e.CreatedBy).HasColumnType("bigint(20)");

                entity.Property(e => e.CreatedDate).HasColumnType("datetime(3)");

                entity.Property(e => e.CreatedDateInt).HasColumnType("bigint(20)");

                entity.Property(e => e.IsDelete).HasColumnType("bit(1)");

                entity.Property(e => e.MemoId).HasColumnType("bigint(20)");

                entity.Property(e => e.TransactionId).HasColumnType("bigint(20)");

                entity.HasOne(d => d.Bill)
                    .WithMany(p => p.Memobills)
                    .HasForeignKey(d => d.BillId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_MemoBills_BillMaster");

                entity.HasOne(d => d.Memo)
                    .WithMany(p => p.Memobills)
                    .HasForeignKey(d => d.MemoId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_MemoBills_Memo");
            });

            modelBuilder.Entity<MstBusinessunit>(entity =>
            {
                entity.ToTable("mst_businessunit");

                entity.Property(e => e.Id).HasColumnType("bigint(20)");

                entity.Property(e => e.CreatedBy).HasColumnType("bigint(20)");

                entity.Property(e => e.CreatedDate).HasColumnType("datetime");

                entity.Property(e => e.IsActive).HasColumnType("bit(1)");

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasColumnType("varchar(50)");
            });

            modelBuilder.Entity<MstCostcenter>(entity =>
            {
                entity.ToTable("mst_costcenter");

                entity.HasIndex(e => e.BusinessUnitid)
                    .HasName("FK_CC_BU");

                entity.Property(e => e.Id).HasColumnType("bigint(20)");

                entity.Property(e => e.BusinessUnitid).HasColumnType("bigint(1)");

                entity.Property(e => e.CostCenterCode)
                    .IsRequired()
                    .HasColumnType("varchar(50)");

                entity.Property(e => e.CreatedBy).HasColumnType("bigint(20)");

                entity.Property(e => e.CreatedDate).HasColumnType("datetime");

                entity.Property(e => e.IsActive).HasColumnType("bit(1)");

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasColumnType("varchar(50)");

                entity.HasOne(d => d.BusinessUnit)
                    .WithMany(p => p.MstCostcenter)
                    .HasForeignKey(d => d.BusinessUnitid)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_CC_BU");
            });

            modelBuilder.Entity<MstCountry>(entity =>
            {
                entity.ToTable("mst_country");

                entity.HasIndex(e => e.CurrencyId)
                    .HasName("FK_CU_CC");

                entity.Property(e => e.Id).HasColumnType("bigint(20)");

                entity.Property(e => e.CreatedBy).HasColumnType("bigint(20)");

                entity.Property(e => e.CreatedDate).HasColumnType("datetime");

                entity.Property(e => e.CurrencyId).HasColumnType("bigint(20)");

                entity.Property(e => e.IsActive).HasColumnType("bit(1)");

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasColumnType("varchar(50)");

                entity.HasOne(d => d.Currency)
                    .WithMany(p => p.MstCountry)
                    .HasForeignKey(d => d.CurrencyId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_CU_CC");
            });

            modelBuilder.Entity<MstCurrency>(entity =>
            {
                entity.ToTable("mst_currency");

                entity.Property(e => e.Id).HasColumnType("bigint(20)");

                entity.Property(e => e.Code)
                    .IsRequired()
                    .HasColumnType("varchar(50)");

                entity.Property(e => e.CreatedBy).HasColumnType("bigint(20)");

                entity.Property(e => e.CreatedDate).HasColumnType("datetime");

                entity.Property(e => e.IsActive).HasColumnType("bit(1)");

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasColumnType("varchar(50)");
            });

            modelBuilder.Entity<MstDepartment>(entity =>
            {
                entity.ToTable("mst_department");

                entity.HasIndex(e => e.BusinessUnitId)
                    .HasName("FK_Dept_BU");

                entity.Property(e => e.Id).HasColumnType("bigint(20)");

                entity.Property(e => e.BusinessUnitId).HasColumnType("bigint(20)");

                entity.Property(e => e.CreatedBy).HasColumnType("bigint(20)");

                entity.Property(e => e.CreatedDate).HasColumnType("datetime");

                entity.Property(e => e.IsActive).HasColumnType("bit(1)");

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasColumnType("varchar(50)");

                entity.HasOne(d => d.BusinessUnit)
                    .WithMany(p => p.MstDepartment)
                    .HasForeignKey(d => d.BusinessUnitId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Dept_BU");
            });

            modelBuilder.Entity<MstEmployee>(entity =>
            {
                entity.HasKey(e => e.UserId)
                    .HasName("PRIMARY");

                entity.ToTable("mst_employee");

                entity.HasIndex(e => e.CostCenterId)
                    .HasName("FK_Mst_Employee_Mst_CostCenter");

                entity.HasIndex(e => e.DepartmentId)
                    .HasName("FK_Mst_Employee_Mst_Department");

                entity.HasIndex(e => e.LineManagerId)
                    .HasName("FK_Mst_EmployeeLinemanager_Mst_Employee");

                entity.HasIndex(e => e.RoleId)
                    .HasName("MstEmployee_RoleId");

                entity.Property(e => e.UserId).HasColumnType("bigint(20)");

                entity.Property(e => e.BusinessUnitId).HasColumnType("bigint(20)");

                entity.Property(e => e.CostCenterId).HasColumnType("bigint(20)");

                entity.Property(e => e.CountryId).HasColumnType("bigint(20)");

                entity.Property(e => e.CreatedBy).HasColumnType("bigint(20)");

                entity.Property(e => e.CreatedDate).HasColumnType("datetime(3)");

                entity.Property(e => e.CreatedDateInt).HasColumnType("bigint(20)");

                entity.Property(e => e.DepartmentId).HasColumnType("bigint(20)");

                entity.Property(e => e.Description).HasColumnType("longtext");

                entity.Property(e => e.Designation).HasColumnType("varchar(80)");

                entity.Property(e => e.EmailId).HasColumnType("varchar(150)");

                entity.Property(e => e.EmpPfnumber)
                    .IsRequired()
                    .HasColumnName("EmpPFNumber")
                    .HasColumnType("varchar(50)");

                entity.Property(e => e.ExtensionNumber).HasColumnType("varchar(50)");

                entity.Property(e => e.FullName)
                    .IsRequired()
                    .HasColumnType("varchar(80)");

                entity.Property(e => e.IsActive).HasColumnType("bit(1)");

                entity.Property(e => e.IsDelete).HasColumnType("bit(1)");

                entity.Property(e => e.IsPresidentOffice).HasColumnType("bit(1)");

                entity.Property(e => e.IsSystemUser).HasColumnType("bit(1)");

                entity.Property(e => e.LineManagerId).HasColumnType("bigint(20)");

                entity.Property(e => e.Password)
                    .IsRequired()
                    .HasColumnType("varchar(25)");

                entity.Property(e => e.RoleId).HasColumnType("bigint(20)");

                entity.Property(e => e.TransactionId).HasColumnType("bigint(20)");

                entity.Property(e => e.UpdatedBy).HasColumnType("bigint(20)");

                entity.Property(e => e.UpdatedDate).HasColumnType("datetime");

                entity.Property(e => e.UpdatedDateInt).HasColumnType("bigint(20)");

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

                entity.HasOne(d => d.Role)
                    .WithMany(p => p.MstEmployee)
                    .HasForeignKey(d => d.RoleId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("MstEmployee_RoleId");
            });

            modelBuilder.Entity<MstHandsetdetail>(entity =>
            {
                entity.ToTable("mst_handsetdetail");

                entity.Property(e => e.Id).HasColumnType("bigint(20)");

                entity.Property(e => e.CreatedBy).HasColumnType("bigint(20)");

                entity.Property(e => e.CreatedDate).HasColumnType("datetime(3)");

                entity.Property(e => e.CreatedDateInt).HasColumnType("bigint(20)");

                entity.Property(e => e.IsDelete).HasColumnType("bit(1)");

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasColumnType("varchar(50)");

                entity.Property(e => e.TransactionId).HasColumnType("bigint(20)");

                entity.Property(e => e.UpdatedBy).HasColumnType("bigint(20)");

                entity.Property(e => e.UpdatedDate).HasColumnType("datetime");

                entity.Property(e => e.UpdatedDateInt).HasColumnType("bigint(20)");
            });

            modelBuilder.Entity<MstInternetdevicedetail>(entity =>
            {
                entity.ToTable("mst_internetdevicedetail");

                entity.Property(e => e.Id).HasColumnType("bigint(20)");

                entity.Property(e => e.CreatedBy).HasColumnType("bigint(20)");

                entity.Property(e => e.CreatedDate).HasColumnType("datetime(3)");

                entity.Property(e => e.CreatedDateInt).HasColumnType("bigint(20)");

                entity.Property(e => e.IsDelete).HasColumnType("bit(1)");

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasColumnType("varchar(50)");

                entity.Property(e => e.TransactionId).HasColumnType("bigint(20)");

                entity.Property(e => e.UpdatedBy).HasColumnType("bigint(20)");

                entity.Property(e => e.UpdatedDate).HasColumnType("datetime");

                entity.Property(e => e.UpdatedDateInt).HasColumnType("bigint(20)");
            });

            modelBuilder.Entity<MstLink>(entity =>
            {
                entity.HasKey(e => e.LinkId)
                    .HasName("PRIMARY");

                entity.ToTable("mst_link");

                entity.HasIndex(e => e.ModuleId)
                    .HasName("MstLink_MstModule");

                entity.Property(e => e.LinkId).HasColumnType("bigint(20)");

                entity.Property(e => e.CreatedBy).HasColumnType("bigint(20)");

                entity.Property(e => e.CreatedDate).HasColumnType("datetime");

                entity.Property(e => e.CreatedDateInt).HasColumnType("bigint(20)");

                entity.Property(e => e.IsActive).HasColumnType("bit(1)");

                entity.Property(e => e.IsSinglePage).HasColumnType("bit(1)");

                entity.Property(e => e.ModuleId).HasColumnType("bigint(20)");

                entity.Property(e => e.ParentId).HasColumnType("bigint(20)");

                entity.Property(e => e.RouteLink)
                    .IsRequired()
                    .HasColumnType("varchar(150)");

                entity.Property(e => e.Title)
                    .IsRequired()
                    .HasColumnType("varchar(150)");

                entity.Property(e => e.UpdatedBy).HasColumnType("bigint(20)");

                entity.Property(e => e.UpdatedDate).HasColumnType("datetime");

                entity.Property(e => e.UpdatedDateInt).HasColumnType("bigint(20)");

                entity.Property(e => e.ViewIndex).HasColumnType("bigint(20)");

                entity.HasOne(d => d.Module)
                    .WithMany(p => p.MstLink)
                    .HasForeignKey(d => d.ModuleId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("MstLink_MstModule");
            });

            modelBuilder.Entity<MstModule>(entity =>
            {
                entity.HasKey(e => e.ModuleId)
                    .HasName("PRIMARY");

                entity.ToTable("mst_module");

                entity.Property(e => e.ModuleId).HasColumnType("bigint(20)");

                entity.Property(e => e.CreatedBy).HasColumnType("bigint(20)");

                entity.Property(e => e.CreatedDate).HasColumnType("datetime");

                entity.Property(e => e.CreatedDateInt).HasColumnType("bigint(20)");

                entity.Property(e => e.IconName)
                    .IsRequired()
                    .HasColumnType("varchar(100)");

                entity.Property(e => e.ModuleName)
                    .IsRequired()
                    .HasColumnType("varchar(50)");

                entity.Property(e => e.ViewIndex).HasColumnType("int(11)");
            });

            modelBuilder.Entity<MstRequestaction>(entity =>
            {
                entity.HasKey(e => e.RequestId)
                    .HasName("PRIMARY");

                entity.ToTable("mst_requestaction");

                entity.Property(e => e.RequestId).HasColumnType("bigint(20)");

                entity.Property(e => e.Text)
                    .IsRequired()
                    .HasColumnType("longtext");
            });

            modelBuilder.Entity<MstRole>(entity =>
            {
                entity.HasKey(e => e.RoleId)
                    .HasName("PRIMARY");

                entity.ToTable("mst_role");

                entity.Property(e => e.RoleId).HasColumnType("bigint(20)");

                entity.Property(e => e.CreatedBy).HasColumnType("bigint(20)");

                entity.Property(e => e.CreatedDate).HasColumnType("datetime");

                entity.Property(e => e.CreatedDateInt).HasColumnType("bigint(20)");

                entity.Property(e => e.IsActive).HasColumnType("bit(1)");

                entity.Property(e => e.IsDelete).HasColumnType("bit(1)");

                entity.Property(e => e.RoleName)
                    .IsRequired()
                    .HasColumnType("varchar(50)");

                entity.Property(e => e.TransactionId).HasColumnType("bigint(20)");

                entity.Property(e => e.UpdatedBy).HasColumnType("bigint(20)");

                entity.Property(e => e.UpdatedDate).HasColumnType("datetime");

                entity.Property(e => e.UpdatedDateInt).HasColumnType("bigint(20)");
            });

            modelBuilder.Entity<MstRolerights>(entity =>
            {
                entity.HasKey(e => e.RoleRightId)
                    .HasName("PRIMARY");

                entity.ToTable("mst_rolerights");

                entity.HasIndex(e => e.RoleId)
                    .HasName("FK_Role");

                entity.Property(e => e.RoleRightId).HasColumnType("bigint(20)");

                entity.Property(e => e.CreatedBy).HasColumnType("bigint(20)");

                entity.Property(e => e.CreatedDate).HasColumnType("datetime(3)");

                entity.Property(e => e.CreatedDateInt).HasColumnType("bigint(20)");

                entity.Property(e => e.HaveFullAccess).HasColumnType("bit(1)");

                entity.Property(e => e.IsAdd).HasColumnType("bit(1)");

                entity.Property(e => e.IsChangeStatus).HasColumnType("bit(1)");

                entity.Property(e => e.IsDelete).HasColumnType("bit(1)");

                entity.Property(e => e.IsEdit).HasColumnType("bit(1)");

                entity.Property(e => e.IsView).HasColumnType("bit(1)");

                entity.Property(e => e.IsReadOnly).HasColumnType("bit(1)");

				entity.Property(e => e.IsEditable).HasColumnType("bit(1)");

				entity.Property(e => e.LinkId).HasColumnType("bigint(20)");

                entity.Property(e => e.RoleId).HasColumnType("bigint(20)");

                entity.Property(e => e.TransactionId).HasColumnType("bigint(20)");

                entity.HasOne(d => d.Role)
                    .WithMany(p => p.MstRolerights)
                    .HasForeignKey(d => d.RoleId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Role");
            });

			modelBuilder.Entity<Notificationlog>(entity =>
			{
				entity.ToTable("notificationlog");

				entity.HasIndex(e => e.ActionUserId)
					.HasName("FK_NotificationLog_ActionMstEmployee");

				entity.HasIndex(e => e.NotificationTypeId)
					.HasName("FK_NotificationLog_NotificationType");

				entity.HasIndex(e => e.UserId)
					.HasName("FK_NotificaitonLog_MstEmployee");

				entity.Property(e => e.Id).HasColumnType("bigint(20)");

				entity.Property(e => e.ActionUserId).HasColumnType("bigint(20)");

				entity.Property(e => e.CreatedDate).HasColumnType("datetime");

				entity.Property(e => e.CreatedDateInt).HasColumnType("bigint(20)");

				entity.Property(e => e.EmployeeBillIormemoId)
					.HasColumnName("EmployeeBillIORMemoId")
					.HasColumnType("bigint(20)");

				entity.Property(e => e.IsDeleted).HasColumnType("bit(1)");

				entity.Property(e => e.IsReadNotification).HasColumnType("bit(1)");

				entity.Property(e => e.NotificationText).HasColumnType("varchar(255)");

				entity.Property(e => e.NotificationTypeId).HasColumnType("bigint(20)");

				entity.Property(e => e.UserId).HasColumnType("bigint(20)");

				entity.HasOne(d => d.ActionUser)
					.WithMany(p => p.NotificationlogActionUser)
					.HasForeignKey(d => d.ActionUserId)
					.HasConstraintName("FK_NotificationLog_ActionMstEmployee");

				entity.HasOne(d => d.NotificationType)
					.WithMany(p => p.Notificationlog)
					.HasForeignKey(d => d.NotificationTypeId)
					.OnDelete(DeleteBehavior.ClientSetNull)
					.HasConstraintName("FK_NotificationLog_NotificationType");

				entity.HasOne(d => d.User)
					.WithMany(p => p.NotificationlogUser)
					.HasForeignKey(d => d.UserId)
					.OnDelete(DeleteBehavior.ClientSetNull)
					.HasConstraintName("FK_NotificaitonLog_MstEmployee");
			});

			modelBuilder.Entity<Operatorcalllog>(entity =>
            {
                entity.ToTable("operatorcalllog");

                entity.HasIndex(e => e.CallTypeId)
                    .HasName("FK_OperatorCallLog_Fix_CallType");

                entity.HasIndex(e => e.EmployeeId)
                    .HasName("FK_OperatorCallLog_Mst_Employee");

                entity.HasIndex(e => e.ProviderId)
                    .HasName("FK_OperatorCallLog_Provider");

                entity.Property(e => e.Id).HasColumnType("bigint(20)");

                entity.Property(e => e.CallDate).HasColumnType("datetime(3)");

                entity.Property(e => e.CallDateInt).HasColumnType("bigint(20)");

                entity.Property(e => e.CallTypeId).HasColumnType("int(20)");

                entity.Property(e => e.CreatedBy).HasColumnType("bigint(20)");

                entity.Property(e => e.CreatedDate).HasColumnType("datetime(3)");

                entity.Property(e => e.CreatedDateInt).HasColumnType("bigint(20)");

                entity.Property(e => e.DialedNumber)
                    .IsRequired()
                    .HasColumnType("varchar(50)");

                entity.Property(e => e.EmpPfnumber)
                    .IsRequired()
                    .HasColumnName("EmpPFNumber")
                    .HasColumnType("varchar(50)");

                entity.Property(e => e.EmployeeId).HasColumnType("bigint(20)");

                entity.Property(e => e.ExtensionNumber).HasColumnType("varchar(50)");

                entity.Property(e => e.IsDelete).HasColumnType("bit(1)");

                entity.Property(e => e.ProviderId).HasColumnType("bigint(20)");

                entity.Property(e => e.TransactionId).HasColumnType("bigint(20)");

                entity.Property(e => e.UpdatedBy).HasColumnType("bigint(20)");

                entity.Property(e => e.UpdatedDate).HasColumnType("datetime");

                entity.Property(e => e.UpdatedDateInt).HasColumnType("bigint(20)");

                entity.HasOne(d => d.CallType)
                    .WithMany(p => p.Operatorcalllog)
                    .HasForeignKey(d => d.CallTypeId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_OperatorCallLog_Fix_CallType");

                entity.HasOne(d => d.Employee)
                    .WithMany(p => p.Operatorcalllog)
                    .HasForeignKey(d => d.EmployeeId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_OperatorCallLog_Mst_Employee");

                entity.HasOne(d => d.Provider)
                    .WithMany(p => p.Operatorcalllog)
                    .HasForeignKey(d => d.ProviderId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_OperatorCallLog_Provider");
            });

            modelBuilder.Entity<Provider>(entity =>
            {
                entity.ToTable("provider");

                entity.HasIndex(e => e.CountryId)
                    .HasName("FK_P_CO");

                entity.HasIndex(e => e.CurrencyId)
                    .HasName("FK_P_CU");

                entity.Property(e => e.Id).HasColumnType("bigint(20)");

                entity.Property(e => e.AccountNumber)
                    .IsRequired()
                    .HasColumnType("varchar(100)");

                entity.Property(e => e.Bank)
                    .IsRequired()
                    .HasColumnType("varchar(100)");

                entity.Property(e => e.ContractNumber)
                    .IsRequired()
                    .HasColumnType("varchar(50)");

                entity.Property(e => e.CountryId).HasColumnType("bigint(20)");

                entity.Property(e => e.CreatedBy).HasColumnType("bigint(20)");

                entity.Property(e => e.CreatedDate).HasColumnType("datetime(3)");

                entity.Property(e => e.CreatedDateInt).HasColumnType("bigint(20)");

                entity.Property(e => e.CurrencyId).HasColumnType("bigint(20)");

                entity.Property(e => e.Ibancode)
                    .IsRequired()
                    .HasColumnName("IBANCode")
                    .HasColumnType("varchar(100)");

                entity.Property(e => e.IsActive).HasColumnType("bit(1)");

                entity.Property(e => e.IsDelete).HasColumnType("bit(1)");

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasColumnType("varchar(100)");

                entity.Property(e => e.ProviderEmail)
                    .IsRequired()
                    .HasColumnType("varchar(50)");

                entity.Property(e => e.Swiftcode)
                    .IsRequired()
                    .HasColumnName("SWIFTCode")
                    .HasColumnType("varchar(100)");

                entity.Property(e => e.TransactionId).HasColumnType("bigint(20)");

                entity.Property(e => e.UpdatedBy).HasColumnType("bigint(20)");

                entity.Property(e => e.UpdatedDate).HasColumnType("datetime");

                entity.Property(e => e.UpdatedDateInt).HasColumnType("bigint(20)");

                entity.HasOne(d => d.Country)
                    .WithMany(p => p.Provider)
                    .HasForeignKey(d => d.CountryId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_P_CO");

                entity.HasOne(d => d.Currency)
                    .WithMany(p => p.Provider)
                    .HasForeignKey(d => d.CurrencyId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_P_CU");
            });

            modelBuilder.Entity<Providercontactdetail>(entity =>
            {
                entity.ToTable("providercontactdetail");

                entity.HasIndex(e => e.ProviderId)
                    .HasName("FK_PCD_P");

                entity.Property(e => e.Id).HasColumnType("bigint(20)");

                entity.Property(e => e.ContactNumbers)
                    .IsRequired()
                    .HasColumnType("varchar(50)");

                entity.Property(e => e.Email)
                    .IsRequired()
                    .HasColumnType("varchar(50)");

                entity.Property(e => e.IsDeleted).HasColumnType("bit(1)");

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasColumnType("varchar(50)");

                entity.Property(e => e.Position).HasColumnType("varchar(50)");

                entity.Property(e => e.ProviderId).HasColumnType("bigint(20)");

                entity.Property(e => e.Title).HasColumnType("varchar(50)");

                entity.HasOne(d => d.Provider)
                    .WithMany(p => p.Providercontactdetail)
                    .HasForeignKey(d => d.ProviderId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_PCD_P");
            });

            modelBuilder.Entity<Providerpackage>(entity =>
            {
                entity.ToTable("providerpackage");

                entity.HasIndex(e => e.InternetDeviceId)
                    .HasName("FK_PP_IDD");

                entity.HasIndex(e => e.ProviderId)
                    .HasName("FK_PP_P");

                entity.HasIndex(e => e.ServiceTypeId)
                    .HasName("FK_PP_ST");

                entity.Property(e => e.Id).HasColumnType("bigint(20)");

                entity.Property(e => e.AdditionalChargeDataAmount)
                    .HasColumnName("AdditionalCharge_DataAmount")
                    .HasColumnType("decimal(20,0)");

                entity.Property(e => e.AdditionalChargeDurationAmount)
                    .HasColumnName("AdditionalCharge_DurationAmount")
                    .HasColumnType("decimal(20,0)");

                entity.Property(e => e.AdditionalChargeMinuteAmount)
                    .HasColumnName("AdditionalCharge_MinuteAmount")
                    .HasColumnType("decimal(20,0)");

                entity.Property(e => e.AdditionalData)
                    .HasColumnName("Additional_Data")
                    .HasColumnType("int(11)");

                entity.Property(e => e.AdditionalMinute)
                    .HasColumnName("Additional_Minute")
                    .HasColumnType("decimal(20,0)");

                entity.Property(e => e.AdditionalMonth)
                    .HasColumnName("Additional_Month")
                    .HasColumnType("int(11)");

                entity.Property(e => e.CreatedBy).HasColumnType("bigint(20)");

                entity.Property(e => e.CreatedDate).HasColumnType("datetime");

                entity.Property(e => e.CreatedDateInt).HasColumnType("bigint(20)");

                entity.Property(e => e.Description).HasColumnType("longtext");

                entity.Property(e => e.DeviceAmount).HasColumnType("decimal(20,0)");

                entity.Property(e => e.DevicePenaltyAmount).HasColumnType("decimal(20,0)");

                entity.Property(e => e.HandsetDetailIds).HasColumnType("longtext");

                entity.Property(e => e.InGroupMinute).HasColumnType("decimal(20,0)");

                entity.Property(e => e.InternationalCallMinute).HasColumnType("decimal(20,0)");

                entity.Property(e => e.InternationalRoamingData).HasColumnType("int(11)");

                entity.Property(e => e.InternationalSharingData).HasColumnType("int(11)");

                entity.Property(e => e.InternetDeviceId).HasColumnType("bigint(20)");

                entity.Property(e => e.IsActive).HasColumnType("bit(1)");

                entity.Property(e => e.IsDelete).HasColumnType("bit(1)");

                entity.Property(e => e.LocalInternetData).HasColumnType("int(11)");

                entity.Property(e => e.LocalMinute).HasColumnType("decimal(20,0)");

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasColumnType("varchar(80)");

                entity.Property(e => e.PackageAmount).HasColumnType("decimal(20,0)");

                entity.Property(e => e.PackageData).HasColumnType("int(11)");

                entity.Property(e => e.PackageMinute).HasColumnType("decimal(10,0)");

                entity.Property(e => e.PackageMonth).HasColumnType("int(11)");

                entity.Property(e => e.ProviderId).HasColumnType("bigint(20)");

                entity.Property(e => e.RoamingMinute).HasColumnType("decimal(20,0)");

                entity.Property(e => e.ServiceTypeId).HasColumnType("bigint(20)");

                entity.Property(e => e.StartDate).HasColumnType("date");

                entity.Property(e => e.StartDateInt).HasColumnType("bigint(20)");

                entity.Property(e => e.TerminationFees).HasColumnType("decimal(20,0)");

                entity.Property(e => e.TransactionId).HasColumnType("bigint(20)");

                entity.Property(e => e.UpdatedBy).HasColumnType("bigint(20)");

                entity.Property(e => e.UpdatedDate).HasColumnType("datetime");

                entity.Property(e => e.UpdatedDateInt).HasColumnType("bigint(20)");

                entity.HasOne(d => d.InternetDevice)
                    .WithMany(p => p.Providerpackage)
                    .HasForeignKey(d => d.InternetDeviceId)
                    .HasConstraintName("FK_PP_IDD");

                entity.HasOne(d => d.Provider)
                    .WithMany(p => p.Providerpackage)
                    .HasForeignKey(d => d.ProviderId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_PP_P");

                entity.HasOne(d => d.ServiceType)
                    .WithMany(p => p.Providerpackage)
                    .HasForeignKey(d => d.ServiceTypeId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_PP_ST");
            });

            modelBuilder.Entity<Providerservice>(entity =>
            {
                entity.ToTable("providerservice");

                entity.HasIndex(e => e.ProviderId)
                    .HasName("FK_PS_P");

                entity.HasIndex(e => e.ServiceTypeId)
                    .HasName("FK_PS_FST");

                entity.Property(e => e.Id).HasColumnType("bigint(20)");

                entity.Property(e => e.IsDelete).HasColumnType("bit(1)");

                entity.Property(e => e.ProviderId).HasColumnType("bigint(20)");

                entity.Property(e => e.ServiceTypeId).HasColumnType("bigint(20)");

                entity.HasOne(d => d.Provider)
                    .WithMany(p => p.Providerservice)
                    .HasForeignKey(d => d.ProviderId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_PS_P");

                entity.HasOne(d => d.ServiceType)
                    .WithMany(p => p.Providerservice)
                    .HasForeignKey(d => d.ServiceTypeId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_PS_FST");
            });

            modelBuilder.Entity<Requesttracelog>(entity =>
            {
                entity.ToTable("requesttracelog");

                entity.Property(e => e.RequestTraceLogId).HasColumnType("bigint(20)");

                entity.Property(e => e.ActionId).HasColumnType("bigint(20)");

                entity.Property(e => e.Browser).HasColumnType("varchar(150)");

                entity.Property(e => e.CreatedById).HasColumnType("bigint(20)");

                entity.Property(e => e.CreatedDate).HasColumnType("datetime(3)");

                entity.Property(e => e.CreatedDateInt).HasColumnType("bigint(20)");

                entity.Property(e => e.Description).HasColumnType("longtext");

                entity.Property(e => e.DeviceId).HasColumnType("varchar(500)");

                entity.Property(e => e.Gcmid)
                    .HasColumnName("GCMID")
                    .HasColumnType("varchar(500)");

                entity.Property(e => e.Ipaddress)
                    .HasColumnName("IPAddress")
                    .HasColumnType("varchar(50)");

                entity.Property(e => e.IsMobile).HasColumnType("bit(1)");

                entity.Property(e => e.Os)
                    .HasColumnName("OS")
                    .HasColumnType("varchar(500)");

                entity.Property(e => e.PhoneModel).HasColumnType("varchar(500)");

                entity.Property(e => e.TransactionId).HasColumnType("bigint(20)");

                entity.Property(e => e.Version).HasColumnType("varchar(50)");
            });

            modelBuilder.Entity<Skypeexceldetail>(entity =>
            {
                entity.ToTable("skypeexceldetail");

                entity.HasIndex(e => e.CurrencyId)
                    .HasName("FK_SkyExDet_CU");

                entity.HasIndex(e => e.ExcelUploadLogId)
                    .HasName("FK_SkyExDet_ExUploadLOg");

                entity.HasIndex(e => e.ServiceTypeId)
                    .HasName("FK_SkyExDet_FST");

                entity.Property(e => e.Id).HasColumnType("bigint(20)");

                entity.Property(e => e.AssignType).HasColumnType("bigint(20)");

                entity.Property(e => e.CallAmount).HasColumnType("decimal(18,2)");

                entity.Property(e => e.CallDate).HasColumnType("datetime(3)");

                entity.Property(e => e.CallDuration).HasColumnType("bigint(20)");

                entity.Property(e => e.CallerNumber).HasColumnType("varchar(20)");

                entity.Property(e => e.CurrencyId).HasColumnType("bigint(20)");

                entity.Property(e => e.Description).HasColumnType("longtext");

                entity.Property(e => e.ExcelUploadLogId).HasColumnType("bigint(20)");

                entity.Property(e => e.IsMatched).HasColumnType("bit(1)");

                entity.Property(e => e.ReceiverNumber).HasColumnType("varchar(20)");

                entity.Property(e => e.ServiceTypeId).HasColumnType("bigint(20)");

                entity.HasOne(d => d.Currency)
                    .WithMany(p => p.Skypeexceldetail)
                    .HasForeignKey(d => d.CurrencyId)
                    .HasConstraintName("FK_SkyExDet_CU");

                entity.HasOne(d => d.ExcelUploadLog)
                    .WithMany(p => p.Skypeexceldetail)
                    .HasForeignKey(d => d.ExcelUploadLogId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_SkyExDet_ExUploadLOg");

                entity.HasOne(d => d.ServiceType)
                    .WithMany(p => p.Skypeexceldetail)
                    .HasForeignKey(d => d.ServiceTypeId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_SkyExDet_FST");
            });

            modelBuilder.Entity<Telephonenumber>(entity =>
            {
                entity.ToTable("telephonenumber");

                entity.HasIndex(e => e.LineTypeId)
                    .HasName("FK_Tele_LineType");

                entity.HasIndex(e => e.ProviderId)
                    .HasName("FK_Tele_Provider");

                entity.Property(e => e.Id).HasColumnType("bigint(20)");

                entity.Property(e => e.AccountNumber).HasColumnType("varchar(20)");

                entity.Property(e => e.CreatedBy).HasColumnType("bigint(20)");

                entity.Property(e => e.CreatedDate).HasColumnType("datetime(3)");

                entity.Property(e => e.CreatedDateInt).HasColumnType("bigint(20)");

                entity.Property(e => e.IsActive).HasColumnType("bit(1)");

                entity.Property(e => e.IsDataRoaming).HasColumnType("bit(1)");

                entity.Property(e => e.IsDelete).HasColumnType("bit(1)");

                entity.Property(e => e.IsInternationalCalls).HasColumnType("bit(1)");

                entity.Property(e => e.IsVoiceRoaming).HasColumnType("bit(1)");

                entity.Property(e => e.LineTypeId).HasColumnType("int(11)");

                entity.Property(e => e.ProviderId).HasColumnType("bigint(20)");

                entity.Property(e => e.TelephoneNumber1)
                    .IsRequired()
                    .HasColumnName("TelephoneNumber")
                    .HasColumnType("varchar(20)");

                entity.Property(e => e.TransactionId).HasColumnType("bigint(20)");

                entity.Property(e => e.UpdatedBy).HasColumnType("bigint(20)");

                entity.Property(e => e.UpdatedDate).HasColumnType("datetime");

                entity.Property(e => e.UpdatedDateInt).HasColumnType("bigint(20)");

                entity.HasOne(d => d.LineType)
                    .WithMany(p => p.Telephonenumber)
                    .HasForeignKey(d => d.LineTypeId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Tele_LineType");

                entity.HasOne(d => d.Provider)
                    .WithMany(p => p.Telephonenumber)
                    .HasForeignKey(d => d.ProviderId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Tele_Provider");
            });

            modelBuilder.Entity<Telephonenumberallocation>(entity =>
            {
                entity.ToTable("telephonenumberallocation");

                entity.HasIndex(e => e.AssignTypeId)
                    .HasName("FK_TeleAllocation_AssigntType");

                entity.HasIndex(e => e.EmployeeId)
                    .HasName("FK_TeleAllocation_EmployeeId");

                entity.HasIndex(e => e.LineStatusId)
                    .HasName("FK_TeleAllocation_LineStatus");

                entity.HasIndex(e => e.TelephoneNumberId)
                    .HasName("FK_TeleAllocation_Telephone");

                entity.Property(e => e.Id).HasColumnType("bigint(20)");

                entity.Property(e => e.AssignTypeId).HasColumnType("bigint(20)");

                entity.Property(e => e.BusinessUnitId).HasColumnType("bigint(20)");

                entity.Property(e => e.CreatedBy).HasColumnType("bigint(20)");

                entity.Property(e => e.CreatedDate).HasColumnType("datetime(3)");

                entity.Property(e => e.CreatedDateInt).HasColumnType("bigint(20)");

                entity.Property(e => e.EmpPfnumber)
                    .IsRequired()
                    .HasColumnName("EmpPFNumber")
                    .HasColumnType("varchar(50)");

                entity.Property(e => e.EmployeeId).HasColumnType("bigint(20)");

                entity.Property(e => e.IsActive).HasColumnType("bit(1)");

                entity.Property(e => e.IsDelete).HasColumnType("bit(1)");

                entity.Property(e => e.LineStatusId).HasColumnType("int(11)");

                entity.Property(e => e.TelephoneNumber)
                    .IsRequired()
                    .HasColumnType("varchar(20)");

                entity.Property(e => e.TelephoneNumberId).HasColumnType("bigint(20)");

                entity.Property(e => e.TransactionId).HasColumnType("bigint(20)");

                entity.Property(e => e.UpdatedBy).HasColumnType("bigint(20)");

                entity.Property(e => e.UpdatedDate).HasColumnType("datetime");

                entity.Property(e => e.UpdatedDateInt).HasColumnType("bigint(20)");

                entity.HasOne(d => d.AssignType)
                    .WithMany(p => p.Telephonenumberallocation)
                    .HasForeignKey(d => d.AssignTypeId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_TelephoneNumberAllocation_Fix_AssignType");

                entity.HasOne(d => d.Employee)
                    .WithMany(p => p.Telephonenumberallocation)
                    .HasForeignKey(d => d.EmployeeId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_TeleAllocation_EmployeeId");

                entity.HasOne(d => d.LineStatus)
                    .WithMany(p => p.Telephonenumberallocation)
                    .HasForeignKey(d => d.LineStatusId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_TeleAllocation_LineStatus");

                entity.HasOne(d => d.TelephoneNumberNavigation)
                    .WithMany(p => p.Telephonenumberallocation)
                    .HasForeignKey(d => d.TelephoneNumberId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_TeleAllocation_Telephone");
            });

            modelBuilder.Entity<Telephonenumberallocationpackage>(entity =>
            {
                entity.ToTable("telephonenumberallocationpackage");

                entity.HasIndex(e => e.PackageId)
                    .HasName("FK_AllocationPackage_Package");

                entity.HasIndex(e => e.TelephoneNumberAllocationId)
                    .HasName("FK_AllocationPakckage_Allocation");

                entity.Property(e => e.Id).HasColumnType("bigint(20)");

                entity.Property(e => e.CreatedBy).HasColumnType("bigint(20)");

                entity.Property(e => e.CreatedDate).HasColumnType("datetime(3)");

                entity.Property(e => e.CreatedDateInt).HasColumnType("bigint(20)");

                entity.Property(e => e.EndDate).HasColumnType("datetime");

                entity.Property(e => e.EndDateInt).HasColumnType("bigint(20)");

                entity.Property(e => e.IsDelete).HasColumnType("bit(1)");

                entity.Property(e => e.PackageId).HasColumnType("bigint(20)");

                entity.Property(e => e.ServiceId).HasColumnType("bigint(20)");

                entity.Property(e => e.StartDate).HasColumnType("datetime");

                entity.Property(e => e.StartDateInt).HasColumnType("bigint(20)");

                entity.Property(e => e.TelephoneNumberAllocationId).HasColumnType("bigint(11)");

                entity.Property(e => e.TransactionId).HasColumnType("bigint(20)");

                entity.Property(e => e.UpdatedBy).HasColumnType("bigint(20)");

                entity.Property(e => e.UpdatedDate).HasColumnType("datetime");

                entity.Property(e => e.UpdatedDateInt).HasColumnType("bigint(20)");

                entity.HasOne(d => d.Package)
                    .WithMany(p => p.Telephonenumberallocationpackage)
                    .HasForeignKey(d => d.PackageId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_AllocationPackage_Package");

                entity.HasOne(d => d.TelephoneNumberAllocation)
                    .WithMany(p => p.Telephonenumberallocationpackage)
                    .HasForeignKey(d => d.TelephoneNumberAllocationId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_AllocationPakckage_Allocation");
            });

            modelBuilder.Entity<Transactiontypesetting>(entity =>
            {
                entity.ToTable("transactiontypesetting");

                entity.HasIndex(e => e.ProviderId)
                    .HasName("FK_TransactionTypeSetting_Provider");

                entity.HasIndex(e => e.SetTypeAs)
                    .HasName("FK_TransactionTypeSetting_FixCallType");

                entity.Property(e => e.Id).HasColumnType("bigint(20)");

                entity.Property(e => e.CreatedBy).HasColumnType("bigint(20)");

                entity.Property(e => e.CreatedDate).HasColumnType("datetime(3)");

                entity.Property(e => e.CreatedDateInt).HasColumnType("bigint(20)");

                entity.Property(e => e.IsDelete).HasColumnType("bit(1)");

				entity.Property(e => e.IsActive).HasColumnType("bit(1)");
					
				entity.Property(e => e.ProviderId).HasColumnType("bigint(20)");

                entity.Property(e => e.SetTypeAs)
                    .HasColumnName("SetTypeAS")
                    .HasColumnType("int(20)");

                entity.Property(e => e.TransactionId).HasColumnType("bigint(20)");

                entity.Property(e => e.TransactionType)
                    .IsRequired()
                    .HasColumnType("varchar(50)");

                entity.Property(e => e.UpdatedBy).HasColumnType("bigint(20)");

                entity.Property(e => e.UpdatedDate).HasColumnType("datetime");

                entity.Property(e => e.UpdatedDateInt).HasColumnType("bigint(20)");

                entity.HasOne(d => d.Provider)
                    .WithMany(p => p.Transactiontypesetting)
                    .HasForeignKey(d => d.ProviderId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_TransactionTypeSetting_Provider");

                entity.HasOne(d => d.SetTypeAsNavigation)
                    .WithMany(p => p.Transactiontypesetting)
                    .HasForeignKey(d => d.SetTypeAs)
                    .HasConstraintName("FK_TransactionTypeSetting_FixCallType");
            });

            modelBuilder.Entity<ExceldetailError>(entity =>
            {
                entity.ToTable("exceldetail_error");

                entity.Property(e => e.Id).HasColumnType("bigint(20)");

                entity.Property(e => e.Bandwidth).HasColumnType("longtext");

                entity.Property(e => e.BusinessUnit).HasColumnType("longtext");

                entity.Property(e => e.CallAmount).HasColumnType("longtext");

                entity.Property(e => e.CallDataKb)
                    .HasColumnName("CallDataKB")
                    .HasColumnType("longtext");

                entity.Property(e => e.CallDate).HasColumnType("longtext");

                entity.Property(e => e.CallDuration).HasColumnType("longtext");

                entity.Property(e => e.CallTime).HasColumnType("longtext");

             

                entity.Property(e => e.CallWithinGroup).HasColumnType("longtext");

                entity.Property(e => e.CallerName).HasColumnType("longtext");

                entity.Property(e => e.CallerNumber).HasColumnType("longtext");

                entity.Property(e => e.CommentOnBandwidth).HasColumnType("longtext");

                entity.Property(e => e.CommentOnPrice).HasColumnType("longtext");

                entity.Property(e => e.CostCentre).HasColumnType("longtext");

                

                entity.Property(e => e.Description).HasColumnType("longtext");

                entity.Property(e => e.Destination).HasColumnType("longtext");

                entity.Property(e => e.FileGuidNo)
                    .IsRequired()
                    .HasColumnType("longtext");

                entity.Property(e => e.ErrorSummary).HasColumnType("longtext");

                entity.Property(e => e.ExcelUploadLogId).HasColumnType("bigint(20)");

                entity.Property(e => e.FinalAnnualChargesKd)
                    .HasColumnName("FinalAnnualChargesKD")
                    .HasColumnType("longtext");

                entity.Property(e => e.GroupDetail).HasColumnType("longtext");

                entity.Property(e => e.GroupId).HasColumnType("longtext");

                entity.Property(e => e.InitialDiscountedAnnualPriceKd)
                    .HasColumnName("InitialDiscountedAnnualPriceKD")
                    .HasColumnType("longtext");

                entity.Property(e => e.InitialDiscountedMonthlyPriceKd)
                    .HasColumnName("InitialDiscountedMonthlyPriceKD")
                    .HasColumnType("longtext");

                entity.Property(e => e.InitialDiscountedSavingMonthlyKd)
                    .HasColumnName("InitialDiscountedSavingMonthlyKD")
                    .HasColumnType("longtext");

                entity.Property(e => e.InitialDiscountedSavingYearlyKd)
                    .HasColumnName("InitialDiscountedSavingYearlyKD")
                    .HasColumnType("longtext");

                entity.Property(e => e.MessageCount).HasColumnType("longtext");

                entity.Property(e => e.MonthlyPrice).HasColumnType("longtext");

                entity.Property(e => e.ReceiverName).HasColumnType("longtext");

                entity.Property(e => e.ReceiverNumber).HasColumnType("longtext");

                entity.Property(e => e.ServiceDetail).HasColumnType("longtext");

                entity.Property(e => e.ServiceTypeId).HasColumnType("bigint(20)");

                entity.Property(e => e.SiteName).HasColumnType("longtext");

                entity.Property(e => e.SubscriptionType).HasColumnType("longtext");

                entity.Property(e => e.TransType).HasColumnType("longtext");
            });

            modelBuilder.Entity<ExceldetailpbxError>(entity =>
            {
                entity.ToTable("exceldetailpbx_error");

                entity.Property(e => e.Id).HasColumnType("bigint(20)");

                entity.Property(e => e.Band).HasColumnType("longtext");

                entity.Property(e => e.CallAmount).HasColumnType("longtext");

                entity.Property(e => e.CallDate).HasColumnType("longtext");

                entity.Property(e => e.CallDuration).HasColumnType("longtext");

                entity.Property(e => e.CallTime).HasColumnType("longtext");

                entity.Property(e => e.CallType).HasColumnType("longtext");

                entity.Property(e => e.ClassificationCode).HasColumnType("longtext");

                entity.Property(e => e.CodeNumber).HasColumnType("longtext");

                entity.Property(e => e.ConnectingParty).HasColumnType("longtext");

                entity.Property(e => e.CurrencyId).HasColumnType("longtext");

                entity.Property(e => e.Description).HasColumnType("longtext");

                entity.Property(e => e.DestinationType).HasColumnType("longtext");

                entity.Property(e => e.DistantNumber).HasColumnType("longtext");

                entity.Property(e => e.FileGuidNo)
                    .IsRequired()
                    .HasColumnType("longtext");

                entity.Property(e => e.ErrorSummary).HasColumnType("longtext");

                entity.Property(e => e.ExcelUploadLogId).HasColumnType("bigint(20)");

                entity.Property(e => e.Name1).HasColumnType("longtext");

                entity.Property(e => e.Name2).HasColumnType("longtext");

                entity.Property(e => e.Name3).HasColumnType("longtext");

                entity.Property(e => e.Name4).HasColumnType("longtext");

                entity.Property(e => e.OtherParty).HasColumnType("longtext");

                entity.Property(e => e.Place).HasColumnType("longtext");

                entity.Property(e => e.Rate).HasColumnType("longtext");

                entity.Property(e => e.RingingTime).HasColumnType("longtext");
            });

            modelBuilder.Entity<SkypeexceldetailError>(entity =>
            {
                entity.ToTable("skypeexceldetail_error");

                entity.Property(e => e.Id).HasColumnType("bigint(20)");

                entity.Property(e => e.CallAmount).HasColumnType("longtext");

                entity.Property(e => e.CallDate).HasColumnType("longtext");

                entity.Property(e => e.CallDuration).HasColumnType("longtext");

                entity.Property(e => e.CallTime).HasColumnType("longtext");

                entity.Property(e => e.CallerNumber).HasColumnType("longtext");

                entity.Property(e => e.Description).HasColumnType("longtext");

                entity.Property(e => e.FileGuidNo)
                    .IsRequired()
                    .HasColumnType("longtext");

                entity.Property(e => e.ErrorSummary).HasColumnType("longtext");

                entity.Property(e => e.ExcelUploadLogId).HasColumnType("bigint(20)");

                entity.Property(e => e.ReceiverNumber).HasColumnType("longtext");

                entity.Property(e => e.ServiceTypeId).HasColumnType("bigint(20)");
            });
        }
    }
}
