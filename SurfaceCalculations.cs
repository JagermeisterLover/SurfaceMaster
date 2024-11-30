public class SurfaceCalculations
{
    public double CalculateEvenAsphereSag(double r, double R, double k, double A4, double A6, double A8, double A10, double A12, double A14, double A16, double A18, double A20)
    {
        return (Math.Pow(r, 2) / (R * (1 + Math.Sqrt(1 - (1 + k) * Math.Pow(r, 2) / Math.Pow(R, 2))))) +
               A4 * Math.Pow(r, 4) + A6 * Math.Pow(r, 6) + A8 * Math.Pow(r, 8) + A10 * Math.Pow(r, 10) +
               A12 * Math.Pow(r, 12) + A14 * Math.Pow(r, 14) + A16 * Math.Pow(r, 16) + A18 * Math.Pow(r, 18) +
               A20 * Math.Pow(r, 20);
    }
    public double CalculateOddAsphereSag(double r, double R, double k, double A3, double A4, double A5, double A6, double A7, double A8, double A9, double A10, double A11, double A12, double A13, double A14, double A15, double A16, double A17, double A18, double A19, double A20)
    {
        return (Math.Pow(r, 2) / (R * (1 + Math.Sqrt(1 - (1 + k) * Math.Pow(r, 2) / Math.Pow(R, 2))))) +
               A3 * Math.Pow(r, 3) + A4 * Math.Pow(r, 4) + A5 * Math.Pow(r, 5) + A6 * Math.Pow(r, 6) +
               A7 * Math.Pow(r, 7) + A8 * Math.Pow(r, 8) + A9 * Math.Pow(r, 9) + A10 * Math.Pow(r, 10) +
               A11 * Math.Pow(r, 11) + A12 * Math.Pow(r, 12) + A13 * Math.Pow(r, 13) + A14 * Math.Pow(r, 14) +
               A15 * Math.Pow(r, 15) + A16 * Math.Pow(r, 16) + A17 * Math.Pow(r, 17) + A18 * Math.Pow(r, 18) +
               A19 * Math.Pow(r, 19) + A20 * Math.Pow(r, 20);
    }

    public double CalculateOpalUnZSag(double r, double R, double e2, double H, double A3, double A4, double A5, double A6, double A7, double A8, double A9, double A10, double A11, double A12, double A13)
    {
        double z = 0; // Initial guess for z
        double tolerance = 1e-15; // Precision control
        int maxIterations = 1000000; // Number of iterations
        int iteration = 0;

        while (iteration < maxIterations)
        {
            double w = z / H; // Calculate w
            double Q = A3 * Math.Pow(w, 3) + A4 * Math.Pow(w, 4) + A5 * Math.Pow(w, 5) + A6 * Math.Pow(w, 6) +
                       A7 * Math.Pow(w, 7) + A8 * Math.Pow(w, 8) + A9 * Math.Pow(w, 9) + A10 * Math.Pow(w, 10) +
                       A11 * Math.Pow(w, 11) + A12 * Math.Pow(w, 12) + A13 * Math.Pow(w, 13);

            double zNew = ((r * r) + (1 - e2) * (z * z)) / (R * 2) + Q;

            if (Math.Abs(zNew - z) < tolerance)
            {
                z = zNew;
                break;
            }

            z = zNew;
            iteration++;
        }

        // Debug
        Console.WriteLine($"Converged after {iteration} iterations: z = {z}");

        return z;
    }
    public double CalculateOpalUnUSag(double r, double R, double e2, double H, double A2, double A3, double A4, double A5, double A6, double A7, double A8, double A9, double A10, double A11, double A12)
    {
        double z = 0; // Initial guess for z
        double tolerance = 1e-15; // Precision control
        int maxIterations = 1000000; // Number of iterations
        int iteration = 0;

        while (iteration < maxIterations)
        {
            double w = Math.Pow(r, 2) / Math.Pow(H, 2); // Calculate w
            double Q = A2 * Math.Pow(w, 2) + A3 * Math.Pow(w, 3) + A4 * Math.Pow(w, 4) + A5 * Math.Pow(w, 5) +
                       A6 * Math.Pow(w, 6) + A7 * Math.Pow(w, 7) + A8 * Math.Pow(w, 8) + A9 * Math.Pow(w, 9) +
                       A10 * Math.Pow(w, 10) + A11 * Math.Pow(w, 11) + A12 * Math.Pow(w, 12);

            double zNew = ((r * r) + (1 - e2) * (z * z)) / (R * 2) + Q;

            if (Math.Abs(zNew - z) < tolerance)
            {
                z = zNew;
                break;
            }

            z = zNew;
            iteration++;
        }

        // Debug
        Console.WriteLine($"Converged after {iteration} iterations: z = {z}");

        return z;
    }
    public double CalculatePolySag(double r, double A1, double A2, double A3, double A4, double A5, double A6, double A7, double A8, double A9, double A10, double A11, double A12, double A13)
    {
        double z = 0;
        double tolerance = 1e-9; // Precision tolerance
        int maxIterations = 10000; // number of iterations
        int iteration = 0;

        while (iteration < maxIterations)
        {
            // Calculate the polynomial value and its derivative
            double polynomialValue = A1 * z + A2 * Math.Pow(z, 2) + A3 * Math.Pow(z, 3) + A4 * Math.Pow(z, 4) +
                                     A5 * Math.Pow(z, 5) + A6 * Math.Pow(z, 6) + A7 * Math.Pow(z, 7) +
                                     A8 * Math.Pow(z, 8) + A9 * Math.Pow(z, 9) + A10 * Math.Pow(z, 10) +
                                     A11 * Math.Pow(z, 11) + A12 * Math.Pow(z, 12) + A13 * Math.Pow(z, 13) - Math.Pow(r, 2);

            double derivativeValue = A1 + 2 * A2 * z + 3 * A3 * Math.Pow(z, 2) + 4 * A4 * Math.Pow(z, 3) +
                                     5 * A5 * Math.Pow(z, 4) + 6 * A6 * Math.Pow(z, 5) + 7 * A7 * Math.Pow(z, 6) +
                                     8 * A8 * Math.Pow(z, 7) + 9 * A9 * Math.Pow(z, 8) + 10 * A10 * Math.Pow(z, 9) +
                                     11 * A11 * Math.Pow(z, 10) + 12 * A12 * Math.Pow(z, 11) + 13 * A13 * Math.Pow(z, 12);

            // update z using Newton-Raphson method
            double zNew = z - polynomialValue / derivativeValue;

            // convergence check
            if (Math.Abs(zNew - z) < tolerance)
            {
                z = zNew;
                break;
            }

            z = zNew;
            iteration++;
        }

        // Debug
        Console.WriteLine($"Converged after {iteration} iterations: z = {z}");

        return z;
    }
    // Other calculation methods...
}