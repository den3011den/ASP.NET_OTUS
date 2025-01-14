using Microsoft.AspNetCore.Mvc;
using PromoCodeFactory.Core.Abstractions.Repositories;
using PromoCodeFactory.Core.Domain.Administration;
using PromoCodeFactory.WebHost.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PromoCodeFactory.WebHost.Controllers
{
    /// <summary>
    /// Сотрудники
    /// </summary>
    [ApiController]
    [Route("api/v1/[controller]")]
    public class EmployeesController : ControllerBase
    {
        private readonly IRepository<Employee> _employeeRepository;
        private readonly IRepository<Role> _rolesRepository;

        public EmployeesController(IRepository<Employee> employeeRepository, IRepository<Role> rolesRepository)
        {
            _employeeRepository = employeeRepository;
            _rolesRepository = rolesRepository;
        }

        /// <summary>
        /// Получить данные всех сотрудников
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<List<EmployeeShortResponse>> GetEmployeesAsync()
        {
            var employees = await _employeeRepository.GetAllAsync();

            var employeesModelList = employees.Select(x =>
                new EmployeeShortResponse()
                {
                    Id = x.Id,
                    Email = x.Email,
                    FullName = x.FullName,
                }).ToList();

            return employeesModelList;
        }


        /// <summary>
        /// Получить данные сотрудника по Id
        /// </summary>
        /// <returns></returns>
        [HttpGet("{id:guid}")]
        public async Task<ActionResult<EmployeeResponse>> GetEmployeeByIdAsync(Guid id)
        {
            var employee = await _employeeRepository.GetByIdAsync(id);

            if (employee == null)
                return NotFound("Не найден сотрудник с Id " + id.ToString());

            var employeeModel = new EmployeeResponse()
            {
                Id = employee.Id,
                Email = employee.Email,
                Roles = employee.Roles.Select(x => new RoleItemResponse()
                {
                    Id = x.Id,
                    Name = x.Name,
                    Description = x.Description
                }).ToList(),
                FullName = employee.FullName,
                AppliedPromocodesCount = employee.AppliedPromocodesCount
            };

            return employeeModel;
        }

        /// <summary>
        /// Добавить нового сотрудника
        /// </summary>
        /// <returns>Возвращает объект созданного сотрудника</returns>
        [HttpPut]
        public async Task<ActionResult<EmployeeResponse>> CreateEmployeeAsync(EmployeeRequest employee)
        {
            Guid newId = Guid.NewGuid();
            Employee newEmployeeVar = new()
            {
                Id = newId,
                Email = employee.Email,
                FirstName = employee.FirstName,
                LastName = employee.LastName,
                AppliedPromocodesCount = employee.AppliedPromocodesCount,
                Roles = employee.Roles.Select(x => new Role() { Id = x.Id, Name = x.Name, Description = x.Description }).ToList()
            };

            foreach (var role in newEmployeeVar.Roles)
            {
                var foundRole = await _rolesRepository.GetByIdAsync(role.Id);
                if (foundRole == null)
                {
                    return NotFound("Роль нового сотрудника с Id " + role.Id.ToString() + " не найдена в справочнике ролей");
                }
            }

            var employeeNew = await _employeeRepository.CreateAsync(newEmployeeVar);

            if (employee == null)
                return Conflict("Уже есть сотрудник с Id " + newEmployeeVar.Id.ToString());

            var employeeModel = new EmployeeResponse()
            {
                Id = employeeNew.Id,
                Email = employeeNew.Email,
                Roles = employeeNew.Roles.Select(x => new RoleItemResponse()
                {
                    Id = x.Id,
                    Name = x.Name,
                    Description = x.Description
                }).ToList(),
                FullName = employeeNew.FullName,
                AppliedPromocodesCount = employeeNew.AppliedPromocodesCount
            };
            return employeeModel;
        }

        /// <summary>
        /// Обновить данные сотрудника
        /// </summary>
        /// <returns>Возвращает объект обновлённого сотрудника</returns>
        [HttpPost]
        public async Task<ActionResult<EmployeeResponse>> UpdateEmployeeAsync(Employee employee)
        {

            foreach (var role in employee.Roles)
            {
                var foundRole = await _rolesRepository.GetByIdAsync(role.Id);
                if (foundRole == null)
                {
                    return NotFound("Роль обновляемого сотрудника с Id " + role.Id.ToString() + " не найдена в справочнике ролей");
                }
            }
            var employeeUpdated = await _employeeRepository.UpdateAsync(employee);

            if (employeeUpdated == null)
                return NotFound("Сотрудник с Id " + employee.Id.ToString() + "не найден");  // Не нашли сотрудника с таким Id

            var employeeModel = new EmployeeResponse()
            {
                Id = employeeUpdated.Id,
                Email = employeeUpdated.Email,
                Roles = employeeUpdated.Roles.Select(x => new RoleItemResponse()
                {
                    Name = x.Name,
                    Description = x.Description
                }).ToList(),
                FullName = employeeUpdated.FullName,
                AppliedPromocodesCount = employeeUpdated.AppliedPromocodesCount
            };
            return employeeModel;
        }

        /// <summary>
        /// Удалить сотрудника
        /// </summary>
        /// <returns></returns>
        [HttpDelete("{id:guid}")]
        public async Task<ActionResult<EmployeeResponse>> DeleteEmployeeAsync(Guid id)
        {
            var deletedEmployee = await _employeeRepository.DeleteAsync(id);

            if (deletedEmployee == null)
                return NotFound("Не найден сотрудник с Id " + id.ToString());
            else
                return Ok("Удалён сотрудник с Id " + id.ToString());
        }
    }
}