﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using TeleBillingUtility.ApplicationClass;

namespace TeleBillingRepository.Repository.Employee
{
   public  interface IEmployeeRepository
    {

        /// <summary>
        /// GetUserProfile by Userid
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        Task<EmployeeProfileAC> GetUserProfile(long userId);

        /// <summary>
        /// reset password
        /// </summary>
        /// <param name="employeeProfileDetailAC"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        Task<ResponseAC> resetPassword(EmployeeProfileDetailAC employeeProfileDetailAC, long userId);

        /// <summary>
        /// GetEmployeeList
        /// </summary>
        /// <returns></returns>
        List<EmployeeProfileDetailAC> GetEmployeeList();

        /// <summary>
        /// This method used fo Delete Employee
        /// </summary>
        /// <param name="id"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        Task<bool> DeleteEmployee(long id, long userId);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="roleId"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        Task<bool> ChangeEmployeeStatus(long Id, long userId);

        /// <summary>
        /// check PFNumber is  Unique
        /// </summary>
        /// <param name="EmpPFNumber"></param>
        /// <param name="empId"></param>
        /// <returns> true if unique</returns>
        Task<bool> checkPFNumberUnique(string EmpPFNumber, long empId = 0);

        /// <summary>
        /// This method used for add new Employee detail
        /// </summary>
        /// <param name="MstEmployeeAC"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        Task<ResponseAC> AddEmployee(MstEmployeeAC employee, long userId);

        /// <summary>
        /// Get Employee Details By Id
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        Task<MstEmployeeAC> GetEmployeeById(long userId);

        /// <summary>
        /// this method is used to edit employee
        /// </summary>
        /// <param name="employee"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        Task<ResponseAC> EditEmployee(MstEmployeeAC employee, long userId);

        /// <summary>
        /// checkEmailExists
        /// </summary>
        /// <param name="email"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        Task<bool> checkEmailUnique(string email, long? id = 0);

    }
}
