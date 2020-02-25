using System;
using System.Threading;

namespace SleepingBarber
{
    public class Shop
    {
        public static Semaphore freeSeats = new Semaphore(3, 3); //Barbershop has 3 seats, starts off with no customers currently occupying them.
        public static Semaphore areSeatsAccessable = new Semaphore(1, 1); // A semaphore to prevent 2 customers sitting on the same chair at the same time by preventing
                                                                          //access of critical region which is seats

        public static Customer[] customers = new Customer[3] { null, null, null }; //Customer data of sitting people.
        static void Main(string[] args)
        {
            //We create one barber, and couple of customers to simulate a barbershops daily flow.
            Barber barber = new Barber();
            Customer customer0 = new Customer();
            Customer customer1 = new Customer();
            Customer customer2 = new Customer();
            Customer customer3 = new Customer();
            Customer customer4 = new Customer();
            Customer customer5 = new Customer();
            //We set the threads here.
            Thread barberThread = new Thread(new ThreadStart(Barber.cutHair));
            Thread customerThread0 = new Thread(new ThreadStart(customer0.sitOnSeat));
            Thread customerThread1 = new Thread(new ThreadStart(customer1.sitOnSeat));
            Thread customerThread2 = new Thread(new ThreadStart(customer2.sitOnSeat));
            Thread customerThread3 = new Thread(new ThreadStart(customer3.sitOnSeat));
            Thread customerThread4 = new Thread(new ThreadStart(customer4.sitOnSeat));
            Thread customerThread5 = new Thread(new ThreadStart(customer5.sitOnSeat));
            //Threads fire up, as a barbershop needs a barber, barberThread is started first.
            barberThread.Start();
            customerThread0.Start();
            customerThread1.Start();
            customerThread2.Start();
            customerThread3.Start();
            customerThread4.Start();
            Thread.Sleep(500);      //To simulate a empty barbershop after couple of customers, we make the last customer wait a while before coming in.
            customerThread5.Start();
            Console.ReadKey();
        }

        public class Barber
        {
            public static Semaphore isBusy = new Semaphore(1, 1); //Barber starts off as not working, and has 2 states, 1 = free, 0 = busy
            public static bool sleeping = true; //A public variable to declare the barber sleeping.
            public static int customerIndex = 0; //To keep which seat a customer sits on, edited by checkCustomers function.


            public static void cutHair()
            {
                while (sleeping == false) //Only starts off when barber is not sleeping
                {
                    if (!checkCustomers()) //If there are no customers currently in shop
                    {
                        sleeping = true; //Barber goes to sleep
                        Console.WriteLine("No customers found, barber is sleeping");
                        return; //Ends the function before reaching it's end.
                    }
                    isBusy.WaitOne(); //Since there is a customer, proven by checkCustomers, barber starts to cut the customers hair, declares himself busy.
                    Console.WriteLine("Customer {0} is getting a haircut.", customers[customerIndex].customerID);
                    Thread.Sleep(100); //Haircut

                    isBusy.Release(); //When he is done cutting, he declares that he is free now.
                    Console.WriteLine("Customer {0} has left", customers[customerIndex].customerID);
                    customers[customerIndex] = null; //Customer has left the shop, hence his seat is empty now.
                }
            }

            private static bool checkCustomers() //Looks through seats to find any customers.
            {
                for (int i = 0; i < 3; i++)
                {
                    if (customers[i] != null) //If a customer is found, customerIndex is updated to point at which seat he is sitting on.
                    {
                        customerIndex = i;
                        return true; //Returning that there is a customer
                    }

                }
                return false;

            }
        }

        public class Customer
        {
            private static int totalCustomers = 0; //Simple static customer counter
            public int customerID; // Customer id for printing
            public Customer()
            {
                totalCustomers++; //Increase total customers
                customerID = totalCustomers; // CustomerID is current total customer count.
            }

            public void sitOnSeat() //Main function for customers.
            {
                Console.WriteLine("Customer {0} approaches shop.", customerID);
                areSeatsAccessable.WaitOne(); //Only one customer can sit down at a time to prevent sitting on each other.
                Console.WriteLine("Customer {0} checks the seats", customerID);
                for (int i = 0; i < 3; i++)
                    if (customers[i] == null) //If an empty seat is found,
                    {
                        customers[i] = this; //Customer sits there,
                        Console.WriteLine("Customer {0} has sat down, waiting.", customerID);
                        freeSeats.WaitOne(); //And takes up one seat.
                        checkBarber(i); // Then goes to check barber to see if he is sleeping.

                        return;
                    }
                Console.WriteLine("Customer {0} couldn't find a seat, leaves without a haircut", customerID);
                areSeatsAccessable.Release();



            }
            public void checkBarber(int customerIndex)
            {

                if (Barber.sleeping == true) //If the barber is sleeping
                {
                    Console.WriteLine("Barber got woken up by {0}", customerID);
                    Barber.customerIndex = customerIndex;
                    Barber.sleeping = false; //Wakes up the barber
                    areSeatsAccessable.Release(); //Lets other customers to sit down
                    Barber.cutHair(); //Gets his haircut
                    freeSeats.Release(); //Customer gives up his seat on waiting seats
                    return;
                }
                areSeatsAccessable.Release(); //If barber isn't sleeping, lets other customers come to the shop.

            }


        }
    }
}





