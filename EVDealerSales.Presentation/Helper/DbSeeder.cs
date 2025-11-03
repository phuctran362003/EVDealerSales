using EVDealerSales.Business.Utils;
using EVDealerSales.BusinessObject.Enums;
using EVDealerSales.DataAccess;
using EVDealerSales.DataAccess.Entities;
using Microsoft.EntityFrameworkCore;

namespace EVDealerSales.Presentation.Helper
{
    public static class DbSeeder
    {
        public static async Task SeedUsersAsync(EVDealerSalesDbContext context)
        {
            // apply migrations if not yet applied
            await context.Database.MigrateAsync();

            if (!await context.Users.AnyAsync(u => u.Role == RoleType.DealerManager))
            {
                var passwordHasher = new PasswordHasher();
                var manager = new User
                {
                    FullName = "Manager 1",
                    Email = "manager@gmail.com",
                    PhoneNumber = "0786315267",
                    PasswordHash = passwordHasher.HashPassword("123")!,
                    Role = RoleType.DealerManager,
                };
                await context.Users.AddAsync(manager);
            }

            if (!await context.Users.AnyAsync(u => u.Role == RoleType.DealerStaff))
            {
                var passwordHasher = new PasswordHasher();
                var staff = new User
                {
                    FullName = "Staff 1",
                    Email = "staff@gmail.com",
                    PhoneNumber = "0786315267",
                    PasswordHash = passwordHasher.HashPassword("123")!,
                    Role = RoleType.DealerStaff,
                };
                await context.Users.AddAsync(staff);
            }

            if (!await context.Users.AnyAsync(u => u.Role == RoleType.Customer))
            {
                var passwordHasher = new PasswordHasher();
                var customer = new User
                {
                    FullName = "Customer 1",
                    Email = "customer@gmail.com",
                    PhoneNumber = "0786315267",
                    PasswordHash = passwordHasher.HashPassword("123")!,
                    Role = RoleType.Customer,
                };
                await context.Users.AddAsync(customer);

            }

            await context.SaveChangesAsync();
        }

        public static async Task SeedVehiclesAsync(EVDealerSalesDbContext context)
        {
            if (!await context.Vehicles.AnyAsync())
            {
                var vehicles = new List<Vehicle>
                {
                    new Vehicle
                    {
                        ModelName = "Model S",
                        TrimName = "Plaid",
                        ModelYear = 2025,
                        BasePrice = 89990M,
                        ImageUrl = "https://images.unsplash.com/photo-1580273916550-e323be2ae537?q=80&w=764&auto=format&fit=crop&ixlib=rb-4.1.0&ixid=M3wxMjA3fDB8MHxwaG90by1wYWdlfHx8fGVufDB8fHx8fA%3D%3D",
                        BatteryCapacity = 100,
                        RangeKM = 637,
                        ChargingTime = 45,
                        TopSpeed = 322,
                        Stock = 5,
                        IsActive = true
                    },
                    new Vehicle
                    {
                        ModelName = "iX",
                        TrimName = "M60",
                        ModelYear = 2025,
                        BasePrice = 88900M,
                        ImageUrl = "https://plus.unsplash.com/premium_photo-1664303847960-586318f59035?q=80&w=1074&auto=format&fit=crop&ixlib=rb-4.1.0&ixid=M3wxMjA3fDB8MHxwaG90by1wYWdlfHx8fGVufDB8fHx8fA%3D%3D",
                        BatteryCapacity = 112,
                        RangeKM = 561,
                        ChargingTime = 35,
                        TopSpeed = 250,
                        Stock = 8,
                        IsActive = true
                    },
                    new Vehicle
                    {
                        ModelName = "EQS",
                        TrimName = "580 4MATIC",
                        ModelYear = 2025,
                        BasePrice = 85900M,
                        ImageUrl = "https://plus.unsplash.com/premium_photo-1683134240084-ba074973f75e?q=80&w=1595&auto=format&fit=crop&ixlib=rb-4.1.0&ixid=M3wxMjA3fDB8MHxwaG90by1wYWdlfHx8fGVufDB8fHx8fA%3D%3D",
                        BatteryCapacity = 108,
                        RangeKM = 587,
                        ChargingTime = 31,
                        TopSpeed = 210,
                        Stock = 3,
                        IsActive = true
                    },
                    new Vehicle
                    {
                        ModelName = "Ioniq 6",
                        TrimName = "Limited AWD",
                        ModelYear = 2025,
                        BasePrice = 52600M,
                        ImageUrl = "https://images.unsplash.com/photo-1502877338535-766e1452684a?q=80&w=1172&auto=format&fit=crop&ixlib=rb-4.1.0&ixid=M3wxMjA3fDB8MHxwaG90by1wYWdlfHx8fGVufDB8fHx8fA%3D%3D",
                        BatteryCapacity = 77,
                        RangeKM = 509,
                        ChargingTime = 18,
                        TopSpeed = 230,
                        Stock = 12,
                        IsActive = true
                    },
                    new Vehicle
                    {
                        ModelName = "Mustang Mach-E",
                        TrimName = "California Route 1",
                        ModelYear = 2025,
                        BasePrice = 57800M,
                        ImageUrl = "https://dailymuabanxe.net/wp-content/uploads/2022/06/Ford-Mustang_Mach-E-7.jpg",
                        BatteryCapacity = 99,
                        RangeKM = 491,
                        ChargingTime = 38,
                        TopSpeed = 180,
                        Stock = 7,
                        IsActive = true
                    },
                    new Vehicle
                    {
                        ModelName = "A6 e-tron",
                        TrimName = "Premium",
                        ModelYear = 2025,
                        BasePrice = 75000M,
                        ImageUrl = "https://naoevo.vn/uploads/images/tintuc/2024/T8/audi-a6-e-tron/audi-a6-e-tron-6.jpg",
                        BatteryCapacity = 100,
                        RangeKM = 487,
                        ChargingTime = 25,
                        TopSpeed = 210,
                        Stock = 4,
                        IsActive = true
                    },
                    new Vehicle
                    {
                        ModelName = "Lyriq",
                        TrimName = "Luxury AWD",
                        ModelYear = 2025,
                        BasePrice = 62000M,
                        ImageUrl = "https://image.cnbcfm.com/api/v1/image/108090905-1737582348519-Lyriq-V.JPG?v=1737582364&w=1920&h=1080",
                        BatteryCapacity = 100,
                        RangeKM = 483,
                        ChargingTime = 30,
                        TopSpeed = 200,
                        Stock = 6,
                        IsActive = true
                    },
                    new Vehicle
                    {
                        ModelName = "Taycan",
                        TrimName = "4S",
                        ModelYear = 2025,
                        BasePrice = 86000M,
                        ImageUrl = "https://i1-vnexpress.vnecdn.net/2024/10/18/Porsche-Taycan-Vnexpress-net-11-JPG.jpg?w=2400&h=0&q=100&dpr=1&fit=crop&s=LoskMEDqKHzXgrHyeWd5Ag&t=image",
                        BatteryCapacity = 93,
                        RangeKM = 407,
                        ChargingTime = 22,
                        TopSpeed = 250,
                        Stock = 2,
                        IsActive = true
                    },
                    new Vehicle
                    {
                        ModelName = "Bolt EV",
                        TrimName = "LT",
                        ModelYear = 2025,
                        BasePrice = 32000M,
                        ImageUrl = "https://hips.hearstapps.com/hmg-prod/images/2020-chevrolet-bolt-ev-premier-101-1588025513.jpg",
                        BatteryCapacity = 65,
                        RangeKM = 416,
                        ChargingTime = 30,
                        TopSpeed = 146,
                        Stock = 15,
                        IsActive = true
                    }
                };

                await context.Vehicles.AddRangeAsync(vehicles);
                await context.SaveChangesAsync();
            }
        }

        public static async Task SeedReportDataAsync(EVDealerSalesDbContext context)
        {
            // Get existing users
            var manager = await context.Users.FirstAsync(u => u.Role == RoleType.DealerManager);
            var staff = await context.Users.FirstAsync(u => u.Role == RoleType.DealerStaff);
            var existingCustomer = await context.Users.FirstAsync(u => u.Role == RoleType.Customer);

            // Create additional customers for realistic data
            var passwordHasher = new PasswordHasher();
            var additionalCustomers = new List<User>();

            for (int i = 2; i <= 10; i++)
            {
                if (!await context.Users.AnyAsync(u => u.Email == $"customer{i}@gmail.com"))
                {
                    additionalCustomers.Add(new User
                    {
                        FullName = $"Customer {i}",
                        Email = $"customer{i}@gmail.com",
                        PhoneNumber = $"07863152{i:D2}",
                        PasswordHash = passwordHasher.HashPassword("123")!,
                        Role = RoleType.Customer,
                        CreatedAt = DateTime.Now.AddMonths(-6).AddDays(i * 3)
                    });
                }
            }

            if (additionalCustomers.Any())
            {
                await context.Users.AddRangeAsync(additionalCustomers);
                await context.SaveChangesAsync();
            }

            // Get all customers
            var allCustomers = await context.Users.Where(u => u.Role == RoleType.Customer).ToListAsync();
            var vehicles = await context.Vehicles.ToListAsync();

            // Seed Test Drives (spread over 6 months)
            if (!await context.TestDrives.AnyAsync())
            {
                var testDrives = new List<TestDrive>();
                var random = new Random();

                for (int i = 0; i < 25; i++)
                {
                    var customer = allCustomers[random.Next(allCustomers.Count)];
                    var vehicle = vehicles[random.Next(vehicles.Count)];
                    var daysAgo = random.Next(0, 180);
                    var scheduledDate = DateTime.Now.AddDays(-daysAgo);

                    var status = daysAgo > 7 ? TestDriveStatus.Completed : 
                                 daysAgo > 3 ? TestDriveStatus.Confirmed : 
                                 TestDriveStatus.Pending;

                    testDrives.Add(new TestDrive
                    {
                        CustomerId = customer.Id,
                        VehicleId = vehicle.Id,
                        StaffId = random.Next(2) == 0 ? staff.Id : manager.Id,
                        ScheduledAt = scheduledDate,
                        Status = status,
                        Notes = $"Test drive for {vehicle.ModelName}",
                        ConfirmedAt = status != TestDriveStatus.Pending ? scheduledDate.AddHours(2) : null,
                        CompletedAt = status == TestDriveStatus.Completed ? scheduledDate.AddHours(3) : null,
                        CreatedAt = scheduledDate.AddDays(-2)
                    });
                }

                await context.TestDrives.AddRangeAsync(testDrives);
                await context.SaveChangesAsync();
            }

            // Seed Orders with proper timeline (6 months of data)
            if (!await context.Orders.AnyAsync())
            {
                var random = new Random();
                var orders = new List<Order>();
                var orderDate = DateTime.Now.AddMonths(-6);

                // Create orders spread across 6 months
                for (int month = 0; month < 6; month++)
                {
                    // Create 5-12 orders per month
                    int ordersThisMonth = random.Next(5, 13);

                    for (int i = 0; i < ordersThisMonth; i++)
                    {
                        var customer = allCustomers[random.Next(allCustomers.Count)];
                        var vehicle = vehicles[random.Next(vehicles.Count)];
                        var createdDate = orderDate.AddDays(random.Next(0, 28));

                        var orderNumber = $"ORD-{createdDate:yyyyMMdd}-{i + 1:D4}";

                        // 80% confirmed, 15% pending, 5% cancelled
                        var rand = random.Next(100);
                        var status = rand < 80 ? OrderStatus.Confirmed :
                                   rand < 95 ? OrderStatus.Pending :
                                   OrderStatus.Cancelled;

                        var order = new Order
                        {
                            Id = Guid.NewGuid(),
                            CustomerId = customer.Id,
                            StaffId = random.Next(2) == 0 ? staff.Id : manager.Id,
                            OrderNumber = orderNumber,
                            Status = status,
                            TotalAmount = vehicle.BasePrice,
                            Notes = status == OrderStatus.Cancelled ? "Cancelled by customer" : null,
                            CreatedAt = createdDate
                        };

                        orders.Add(order);
                    }

                    orderDate = orderDate.AddMonths(1);
                }

                await context.Orders.AddRangeAsync(orders);
                await context.SaveChangesAsync();

                // Create OrderItems for each order
                var orderItems = new List<OrderItem>();
                foreach (var order in orders)
                {
                    var vehicle = vehicles[random.Next(vehicles.Count)];
                    orderItems.Add(new OrderItem
                    {
                        OrderId = order.Id,
                        VehicleId = vehicle.Id,
                        UnitPrice = vehicle.BasePrice,
                        CreatedAt = order.CreatedAt
                    });
                }

                await context.OrderItems.AddRangeAsync(orderItems);
                await context.SaveChangesAsync();

                // Create Invoices for each order
                var invoices = new List<Invoice>();
                foreach (var order in orders)
                {
                    var invoiceNumber = $"INV-{order.CreatedAt:yyyyMMdd}-{order.OrderNumber.Split('-').Last()}";
                    var invoiceStatus = order.Status == OrderStatus.Confirmed ? InvoiceStatus.Paid :
                                       order.Status == OrderStatus.Cancelled ? InvoiceStatus.Canceled :
                                       InvoiceStatus.Pending;

                    invoices.Add(new Invoice
                    {
                        OrderId = order.Id,
                        CustomerId = order.CustomerId,
                        InvoiceNumber = invoiceNumber,
                        TotalAmount = order.TotalAmount,
                        Status = invoiceStatus,
                        Notes = invoiceStatus == InvoiceStatus.Paid ? "Paid in full" : "Awaiting payment",
                        CreatedAt = order.CreatedAt
                    });
                }

                await context.Invoices.AddRangeAsync(invoices);
                await context.SaveChangesAsync();

                // Create Payments for paid invoices
                var payments = new List<Payment>();
                var paidInvoices = invoices.Where(i => i.Status == InvoiceStatus.Paid).ToList();

                foreach (var invoice in paidInvoices)
                {
                    payments.Add(new Payment
                    {
                        InvoiceId = invoice.Id,
                        Amount = invoice.TotalAmount,
                        PaymentDate = invoice.CreatedAt.AddDays(1),
                        Status = PaymentStatus.Paid,
                        PaymentIntentId = $"pi_{Guid.NewGuid().ToString().Replace("-", "").Substring(0, 24)}",
                        PaymentMethod = random.Next(3) switch
                        {
                            0 => "card",
                            1 => "bank_transfer",
                            _ => "paypal"
                        },
                        CreatedAt = invoice.CreatedAt.AddDays(1)
                    });
                }

                await context.Payments.AddRangeAsync(payments);
                await context.SaveChangesAsync();

                // Create Deliveries for confirmed orders
                var deliveries = new List<Delivery>();
                var confirmedOrders = orders.Where(o => o.Status == OrderStatus.Confirmed).ToList();

                foreach (var order in confirmedOrders)
                {
                    var plannedDate = order.CreatedAt.AddDays(7);
                    var isDelivered = plannedDate < DateTime.Now.AddDays(-2);
                    var isInTransit = !isDelivered && plannedDate < DateTime.Now.AddDays(3);
                    var actualDate = isDelivered ? plannedDate.AddDays(random.Next(-1, 2)) : (DateTime?)null;

                    var deliveryStatus = isDelivered ? DeliveryStatus.Delivered :
                                       isInTransit ? DeliveryStatus.InTransit :
                                       plannedDate < DateTime.Now ? DeliveryStatus.Scheduled :
                                       DeliveryStatus.Pending;

                    deliveries.Add(new Delivery
                    {
                        OrderId = order.Id,
                        PlannedDate = plannedDate,
                        ActualDate = actualDate,
                        Status = deliveryStatus,
                        ShippingAddress = $"{random.Next(1, 200)} Main St, District {random.Next(1, 13)}, Ho Chi Minh City",
                        Notes = "Standard delivery",
                        StaffNotes = deliveryStatus == DeliveryStatus.Delivered ? "Delivered successfully" : null,
                        CreatedAt = order.CreatedAt.AddDays(2)
                    });
                }

                await context.Deliveries.AddRangeAsync(deliveries);
                await context.SaveChangesAsync();

                // Create Feedbacks for some delivered orders
                var feedbacks = new List<Feedback>();
                var deliveredOrders = confirmedOrders
                    .Where(o => deliveries.Any(d => d.OrderId == o.Id && d.Status == DeliveryStatus.Delivered))
                    .Take(15)
                    .ToList();

                var feedbackTemplates = new[]
                {
                    "Great experience! The vehicle exceeded my expectations.",
                    "Very satisfied with the service and the car quality.",
                    "Professional staff and smooth delivery process.",
                    "The car is amazing, but delivery was a bit delayed.",
                    "Excellent customer service throughout the purchase.",
                    "Happy with my purchase. Highly recommend!",
                    "Good overall experience. Would buy again.",
                    "The staff was very helpful and knowledgeable."
                };

                foreach (var order in deliveredOrders)
                {
                    var isResolved = random.Next(100) < 70; // 70% resolved
                    var feedbackDate = order.CreatedAt.AddDays(random.Next(10, 20));

                    feedbacks.Add(new Feedback
                    {
                        CustomerId = order.CustomerId,
                        OrderId = order.Id,
                        Content = feedbackTemplates[random.Next(feedbackTemplates.Length)],
                        CreatedBy = order.CustomerId,
                        ResolvedBy = isResolved ? manager.Id : null,
                        CreatedAt = feedbackDate,
                        UpdatedAt = isResolved ? feedbackDate.AddDays(1) : feedbackDate
                    });
                }

                await context.Feedbacks.AddRangeAsync(feedbacks);
                await context.SaveChangesAsync();
            }
        }
    }
}
