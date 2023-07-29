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
                DateCompleted = DateTime.Now
            },
            new ServiceTicket
            {
                Id = 3,
                CustomerId = 1,
                EmployeeId = 2,
                Description = "Ticket #3: Software installation problem on user's computer",
                Emergency = false,
                DateCompleted = DateTime.Now
            },
            new ServiceTicket
            {
                Id = 4,
                CustomerId = 2,
                EmployeeId = 1,
                Description = "Ticket #4: Printer not responding in the accounting department",
                Emergency = false,
                DateCompleted = DateTime.Now
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

app.MapGet("/employees", () =>
{
    return employees;
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




app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
