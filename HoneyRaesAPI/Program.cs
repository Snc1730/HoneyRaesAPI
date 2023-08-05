using HoneyRaesAPI.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

List<HoneyRaesAPI.Models.Customer> customers = new List<HoneyRaesAPI.Models.Customer> 
{
    new Customer {Id = 1, Name = "Bob", Address = "1234 Somewhere Road"},
    new Customer {Id = 2, Name = "Todd", Address = "5678 Nowhere Street"},
    new Customer {Id = 3, Name = "Karen", Address = "9876 Sample Street"}
};

List<HoneyRaesAPI.Models.Employee> employees = new List<HoneyRaesAPI.Models.Employee> 
{
    new Employee {Id = 1, Name = "Mr Bean", Specialty = "Accountant" },
    new Employee {Id = 2, Name = "Ms Smith", Specialty = "Secretary"},
    new Employee {Id = 3, Name = "Steve", Specialty = "Janitor"}
};

List<HoneyRaesAPI.Models.ServiceTicket> serviceTickets = new List<HoneyRaesAPI.Models.ServiceTicket> 
{
    new ServiceTicket
            {
                Id = 1,
                CustomerId = 1,
                EmployeeId = null,
                Description = "Ticket #1: The mysterious case of the dancing laptop",
                Emergency = false,
                DateCompleted = DateTime.Now
            },
            new ServiceTicket
            {
                Id = 2,
                CustomerId = 3,
                EmployeeId = 3,
                Description = "Ticket #2: Network connectivity issue in the main office",
                Emergency = true,
                DateCompleted = new DateTime(2023,07,15)
            },
            new ServiceTicket
            {
                Id = 3,
                CustomerId = 1,
                EmployeeId = 2,
                Description = "Ticket #3: Software installation problem on user's computer",
                Emergency = false,
                DateCompleted = new DateTime(2023,07,01)
            },
            new ServiceTicket
            {
                Id = 4,
                CustomerId = 2,
                EmployeeId = 1,
                Description = "Ticket #4: Printer not responding in the accounting department",
                Emergency = false,
                DateCompleted = new DateTime(2020,05,18)
            },
            new ServiceTicket
            {
                Id = 5,
                CustomerId = 3,
                EmployeeId = 2,
                Description = "Ticket #5: Website server outage",
                Emergency = true,
                DateCompleted = null
            }
};

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapGet("/servicetickets", () =>
{
    return serviceTickets;
});

app.MapPost("/servicetickets", (ServiceTicket serviceTicket) =>
{
    // creates a new id (When we get to it later, our SQL database will do this for us like JSON Server did!)
    serviceTicket.Id = serviceTickets.Max(st => st.Id) + 1;
    serviceTickets.Add(serviceTicket);
    return serviceTicket;
});

app.MapGet("/servicetickets/{id}", (int id) =>
{
    ServiceTicket serviceTicket = serviceTickets.FirstOrDefault(st => st.Id == id);
    if (serviceTicket == null)
    {
        return Results.NotFound();
    }
    serviceTicket.Employee = employees.FirstOrDefault(e => e.Id == serviceTicket.EmployeeId);
    serviceTicket.Customer = customers.FirstOrDefault(cu => cu.Id == id);
    return Results.Ok(serviceTicket);
});

app.MapDelete("/servicetickets/{id}", (int id) =>
{
    ServiceTicket serviceTicketToDelete = serviceTickets.FirstOrDefault(st => st.Id == id);
    if (serviceTicketToDelete == null)
    {
        return Results.NotFound("Service ticket not found.");
    }

    serviceTickets.Remove(serviceTicketToDelete);

    return Results.NoContent();
});

app.MapPut("/servicetickets/{id}", (int id, ServiceTicket serviceTicket) =>
{
    ServiceTicket ticketToUpdate = serviceTickets.FirstOrDefault(st => st.Id == id);
    int ticketIndex = serviceTickets.IndexOf(ticketToUpdate);
    if (ticketToUpdate == null)
    {
        return Results.NotFound();
    }
    //the id in the request route doesn't match the id from the ticket in the request body. That's a bad request!
    if (id != serviceTicket.Id)
    {
        return Results.BadRequest();
    }
    serviceTickets[ticketIndex] = serviceTicket;
    return Results.Ok();
});

app.MapPost("/servicetickets/{id}/complete", (int id) =>
{
    ServiceTicket ticketToComplete = serviceTickets.FirstOrDefault(st => st.Id == id);
    if (ticketToComplete == null)
    {
        return Results.NotFound("Service ticket not found.");
    }

    ticketToComplete.DateCompleted = DateTime.Today;

    return Results.NoContent();
});

app.MapGet("/servicetickets/incomplete-emergencies", () =>
{
    var incompleteEmergencies = serviceTickets.Where(st => st.Emergency && st.DateCompleted == null).ToList();

    return Results.Ok(incompleteEmergencies);
});

app.MapGet("/unassignedservicetickets", () =>
{
    var unassignedServiceTickets = serviceTickets.Where(st => st.EmployeeId == null).ToList();

    return Results.Ok(unassignedServiceTickets);
});

app.MapGet("/completedtickets/oldestfirst", () =>
{
    var completedTicketsOldestFirst = serviceTickets
        .Where(st => st.DateCompleted != null)
        .OrderBy(st => st.DateCompleted)
        .ToList();

    return Results.Ok(completedTicketsOldestFirst);
});

app.MapGet("/incompletetickets/order", () =>
{
    var incompleteTickets = serviceTickets
        .Where(st => st.DateCompleted == null)
        .ToList();

    var orderedIncompleteTickets = incompleteTickets
        .OrderByDescending(st => st.Emergency)
        .ThenBy(st => st.EmployeeId == null)
        .ToList();

    return Results.Ok(orderedIncompleteTickets);
});

app.MapGet("/employees", () =>
{
    return employees;
});

app.MapGet("/employeeofthemonth", () =>
{
    DateTime lastMonth = DateTime.Today.AddMonths(-1);

    var employeeOfTheMonth = employees
        .OrderByDescending(emp => serviceTickets.Count(st => st.EmployeeId == emp.Id && st.DateCompleted >= lastMonth))
        .FirstOrDefault();

    return Results.Ok(employeeOfTheMonth);
});

app.MapGet("/employees/{id}", (int id) =>
{
    Employee employee = employees.FirstOrDefault(emp => emp.Id == id);
    if (employee == null)
    {
        return Results.NotFound("No employee found.");
    }

    employee.ServiceTickets = serviceTickets.Where(st => st.EmployeeId == id).ToList();

    return Results.Ok(employee);
});

app.MapGet("/availableEmployees", () =>
{
    var assignedEmployeeIds = serviceTickets
        .Where(st => st.DateCompleted == null && st.EmployeeId != null)
        .Select(st => st.EmployeeId);

    var availableEmployees = employees
        .Where(emp => !assignedEmployeeIds.Contains(emp.Id))
        .ToList();

    return Results.Ok(availableEmployees);
});

app.MapGet("/customers", () =>
{
    return customers;
});

app.MapGet("/customers/{id}", (int id) =>
{
    Customer customer = customers.FirstOrDefault(c => c.Id == id);
    if (customer == null)
    {
        return Results.NotFound("No customer found.");
    }

    // Find the service tickets associated with this customer
    customer.ServiceTickets = serviceTickets.Where(st => st.CustomerId == id).ToList();

    return Results.Ok(customer);
});

app.MapGet("/customers/notclosedforyear", () =>
{
    DateTime oneYearAgo = DateTime.Today.AddYears(-1);

    var customersNotClosedForYear = customers.Where(cust =>
        !serviceTickets.Any(st => st.CustomerId == cust.Id && st.DateCompleted != null && st.DateCompleted >= oneYearAgo)
    ).ToList();

    return Results.Ok(customersNotClosedForYear);
});

app.MapGet("/customers/assignedtoemployee/{employeeId}", (int employeeId) =>
{
    var customerIdsForEmployee = serviceTickets
        .Where(st => st.EmployeeId == employeeId)
        .Select(st => st.CustomerId)
        .Distinct();

    var customersForEmployee = customers
        .Where(cust => customerIdsForEmployee.Contains(cust.Id))
        .ToList();

    return Results.Ok(customersForEmployee);
});

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
