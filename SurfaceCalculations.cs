using System;

public class SurfaceCalculations
{
    public double CalculateEvenAsphereSag(double r, double R, double k,
        double A4, double A6, double A8, double A10, double A12, double A14, double A16, double A18, double A20)
    {
        double baseSag = Math.Pow(r, 2) / (R * (1 + Math.Sqrt(1 - (1 + k) * Math.Pow(r, 2) / Math.Pow(R, 2))));
        return baseSag +
               A4 * Math.Pow(r, 4) + A6 * Math.Pow(r, 6) + A8 * Math.Pow(r, 8) + A10 * Math.Pow(r, 10) +
               A12 * Math.Pow(r, 12) + A14 * Math.Pow(r, 14) + A16 * Math.Pow(r, 16) + A18 * Math.Pow(r, 18) +
               A20 * Math.Pow(r, 20);
    }

    public double CalculateOddAsphereSag(double r, double R, double k,
        double A3, double A4, double A5, double A6, double A7, double A8, double A9, double A10,
        double A11, double A12, double A13, double A14, double A15, double A16, double A17, double A18, double A19, double A20)
    {
        double baseSag = Math.Pow(r, 2) / (R * (1 + Math.Sqrt(1 - (1 + k) * Math.Pow(r, 2) / Math.Pow(R, 2))));
        return baseSag +
               A3 * Math.Pow(r, 3) + A4 * Math.Pow(r, 4) + A5 * Math.Pow(r, 5) + A6 * Math.Pow(r, 6) +
               A7 * Math.Pow(r, 7) + A8 * Math.Pow(r, 8) + A9 * Math.Pow(r, 9) + A10 * Math.Pow(r, 10) +
               A11 * Math.Pow(r, 11) + A12 * Math.Pow(r, 12) + A13 * Math.Pow(r, 13) + A14 * Math.Pow(r, 14) +
               A15 * Math.Pow(r, 15) + A16 * Math.Pow(r, 16) + A17 * Math.Pow(r, 17) + A18 * Math.Pow(r, 18) +
               A19 * Math.Pow(r, 19) + A20 * Math.Pow(r, 20);
    }

    public double CalculateOpalUnUSag(double r, double R, double e2, double H,
        double A2, double A3, double A4, double A5, double A6, double A7, double A8, double A9, double A10, double A11, double A12)
    {
        double z = 0;
        const double tolerance = 1e-15;
        int maxIterations = 1000000, iteration = 0;
        double rSquared = r * r, invR2 = 1.0 / (R * 2);
        double w = rSquared / (H * H);

        while (iteration < maxIterations)
        {
            double wPower = w * w; // start with w^2
            double Q = A2 * wPower;
            double[] coeffs = { A3, A4, A5, A6, A7, A8, A9, A10, A11, A12 };
            for (int i = 0; i < coeffs.Length; i++)
            {
                wPower *= w;
                Q += coeffs[i] * wPower;
            }
            double zNew = (rSquared + (1 - e2) * (z * z)) * invR2 + Q;
            if (Math.Abs(zNew - z) < tolerance)
            {
                z = zNew;
                break;
            }
            z = zNew;
            iteration++;
        }
        Console.WriteLine($"Converged after {iteration} iterations: z = {z}");
        return z;
    }

    public double CalculateOpalUnZSag(
        double r, double R, double e2, double H,
        double A3, double A4, double A5, double A6, double A7, double A8,
        double A9, double A10, double A11, double A12, double A13)
    {
        // Precompute constants
        const double tolerance = 1e-12; // Convergence tolerance
        const int maxIterations = 1000; // Maximum number of iterations
        double c = 1.0 / R; // Curvature
        double rSquared = r * r;

        // Initialize z with a reasonable initial guess
        double z = r / R; // A good starting point
        int iteration = 0;

        while (iteration < maxIterations)
        {
            // Calculate w = z / H
            double w = z / H;

            // Evaluate Q(w) = A3*w^3 + A4*w^4 + ... + A13*w^13 using Horner's method
            double Q = A13;
            Q = Q * w + A12;
            Q = Q * w + A11;
            Q = Q * w + A10;
            Q = Q * w + A9;
            Q = Q * w + A8;
            Q = Q * w + A7;
            Q = Q * w + A6;
            Q = Q * w + A5;
            Q = Q * w + A4;
            Q = Q * w + A3;
            Q *= w * w * w; // Multiply by w^3 at the end

            // Compute the left-hand side of the equation: LHS = z - c * (r^2 + (1 - e^2) * z^2) / 2 - Q
            double lhs = z - (c * (rSquared + (1 - e2) * z * z) / 2) - Q;

            // Compute the derivative of the left-hand side with respect to z
            // Derivative of LHS: dLHS/dz = 1 - c * (1 - e^2) * z - dQ/dz
            double Q_deriv = 0.0;
            if (A3 != 0 || A4 != 0 || A5 != 0 || A6 != 0 || A7 != 0 || A8 != 0
                || A9 != 0 || A10 != 0 || A11 != 0 || A12 != 0 || A13 != 0)
            {
                Q_deriv = A13 * 13;
                Q_deriv = Q_deriv * w + A12 * 12;
                Q_deriv = Q_deriv * w + A11 * 11;
                Q_deriv = Q_deriv * w + A10 * 10;
                Q_deriv = Q_deriv * w + A9 * 9;
                Q_deriv = Q_deriv * w + A8 * 8;
                Q_deriv = Q_deriv * w + A7 * 7;
                Q_deriv = Q_deriv * w + A6 * 6;
                Q_deriv = Q_deriv * w + A5 * 5;
                Q_deriv = Q_deriv * w + A4 * 4;
                Q_deriv = Q_deriv * w + A3 * 3;
                Q_deriv *= w * w; // Multiply by w^2 at the end
                Q_deriv /= H; // Chain rule: dQ/dz = dQ/dw * dw/dz, where dw/dz = 1/H
            }

            double lhs_deriv = 1 - c * (1 - e2) * z - Q_deriv;

            // Newton-Raphson step: z_new = z - LHS / dLHS/dz
            double delta = lhs / lhs_deriv;
            double zNew = z - delta;

            // Check for convergence
            if (Math.Abs(delta) < tolerance)
            {
                Console.WriteLine($"Converged after {iteration + 1} iterations: z = {zNew}");
                return zNew;
            }

            z = zNew;
            iteration++;
        }

        // Handle the case where maximum iterations are reached without convergence
        Console.WriteLine($"Warning: Maximum iterations reached without convergence for CalculateOpalUnZSag. z = {z}");
        return z;
    }

    public double CalculatePolySag(double r,
        double A1, double A2, double A3, double A4, double A5, double A6,
        double A7, double A8, double A9, double A10, double A11, double A12, double A13)
    {
        const double tolerance = 1e-12;
        const int maxIterations = 1000;
        double rSquared = r * r;
        double z = 1.0;
        int iteration = 0;

        while (iteration < maxIterations)
        {
            double Q = A13;
            Q = Q * z + A12;
            Q = Q * z + A11;
            Q = Q * z + A10;
            Q = Q * z + A9;
            Q = Q * z + A8;
            Q = Q * z + A7;
            Q = Q * z + A6;
            Q = Q * z + A5;
            Q = Q * z + A4;
            Q = Q * z + A3;
            Q = Q * z + A2;
            Q = Q * z + A1;

            double P = z * Q;
            double Q_deriv = A13 * 12;
            Q_deriv = Q_deriv * z + A12 * 11;
            Q_deriv = Q_deriv * z + A11 * 10;
            Q_deriv = Q_deriv * z + A10 * 9;
            Q_deriv = Q_deriv * z + A9 * 8;
            Q_deriv = Q_deriv * z + A8 * 7;
            Q_deriv = Q_deriv * z + A7 * 6;
            Q_deriv = Q_deriv * z + A6 * 5;
            Q_deriv = Q_deriv * z + A5 * 4;
            Q_deriv = Q_deriv * z + A4 * 3;
            Q_deriv = Q_deriv * z + A3 * 2;
            Q_deriv = Q_deriv * z + A2;
            double P_deriv = Q + z * Q_deriv;
            double delta = (P - rSquared) / P_deriv;
            double zNew = z - delta;
            if (Math.Abs(delta) < tolerance)
            {
                Console.WriteLine($"Converged after {iteration + 1} iterations: z = {zNew}");
                return zNew;
            }
            z = zNew;
            iteration++;
        }
        Console.WriteLine($"Warning: Maximum iterations reached without convergence for CalculatePolySag. z = {z}");
        return z;
    }

    public double CalculateEvenAsphereSlope(double r, double R, double k,
        double A4, double A6, double A8, double A10, double A12, double A14, double A16, double A18, double A20)
    {
        // Compute Q = sqrt(1 - (1+k)*r^2/R^2)
        double Q = Math.Sqrt(1 - (1 + k) * Math.Pow(r, 2) / Math.Pow(R, 2));

        // Derivative of the base sag:
        // d/dr [r^2/(R*(1+Q))] = [2r(1+Q) + ((1+k)*r^3/(R^2*Q))] / (R*(1+Q)^2)
        double baseSagSlope = (2 * r * (1 + Q) + (1 + k) * Math.Pow(r, 3) / (Math.Pow(R, 2) * Q))
                                / (R * Math.Pow(1 + Q, 2));

        // Derivative of the polynomial (even powers only)
        double polySlope = 4 * A4 * Math.Pow(r, 3) +
                           6 * A6 * Math.Pow(r, 5) +
                           8 * A8 * Math.Pow(r, 7) +
                          10 * A10 * Math.Pow(r, 9) +
                          12 * A12 * Math.Pow(r, 11) +
                          14 * A14 * Math.Pow(r, 13) +
                          16 * A16 * Math.Pow(r, 15) +
                          18 * A18 * Math.Pow(r, 17) +
                          20 * A20 * Math.Pow(r, 19);

        return baseSagSlope + polySlope;
    }

    public double CalculateOddAsphereSlope(double r, double R, double k,
        double A3, double A4, double A5, double A6, double A7, double A8, double A9, double A10,
        double A11, double A12, double A13, double A14, double A15, double A16, double A17, double A18, double A19, double A20)
    {
        // Compute Q = sqrt(1 - (1+k)*r^2/R^2)
        double Q = Math.Sqrt(1 - (1 + k) * Math.Pow(r, 2) / Math.Pow(R, 2));

        // Derivative of the base sag:
        // d/dr [r^2/(R*(1+Q))] = [2r(1+Q) + ((1+k)*r^3/(R^2*Q))] / (R*(1+Q)^2)
        double baseSagSlope = (2 * r * (1 + Q) + (1 + k) * Math.Pow(r, 3) / (Math.Pow(R, 2) * Q))
                                / (R * Math.Pow(1 + Q, 2));

        // Derivative of the polynomial (odd terms included)
        double polySlope = 3 * A3 * Math.Pow(r, 2) +
                           4 * A4 * Math.Pow(r, 3) +
                           5 * A5 * Math.Pow(r, 4) +
                           6 * A6 * Math.Pow(r, 5) +
                           7 * A7 * Math.Pow(r, 6) +
                           8 * A8 * Math.Pow(r, 7) +
                           9 * A9 * Math.Pow(r, 8) +
                          10 * A10 * Math.Pow(r, 9) +
                          11 * A11 * Math.Pow(r, 10) +
                          12 * A12 * Math.Pow(r, 11) +
                          13 * A13 * Math.Pow(r, 12) +
                          14 * A14 * Math.Pow(r, 13) +
                          15 * A15 * Math.Pow(r, 14) +
                          16 * A16 * Math.Pow(r, 15) +
                          17 * A17 * Math.Pow(r, 16) +
                          18 * A18 * Math.Pow(r, 17) +
                          19 * A19 * Math.Pow(r, 18) +
                          20 * A20 * Math.Pow(r, 19);

        return baseSagSlope + polySlope;
    }

    public double CalculateOpalUnUSlope(double r, double R, double e2, double H,
        double A2, double A3, double A4, double A5, double A6, double A7, double A8, double A9, double A10, double A11, double A12)
    {
        double z = CalculateOpalUnUSag(r, R, e2, H, A2, A3, A4, A5, A6, A7, A8, A9, A10, A11, A12);
        double rSquared = r * r;
        double invR2 = 1.0 / (2 * R);
        double w = rSquared / (H * H);

        double dQdr = 0;
        double wPower = w; // start with w
        double[] coeffs = { A2, A3, A4, A5, A6, A7, A8, A9, A10, A11, A12 };
        for (int i = 1; i < coeffs.Length; i++)
        {
            dQdr += i * coeffs[i] * wPower;
            wPower *= w;
        }
        dQdr *= (2 * r) / (H * H);

        return (r * invR2 + dQdr) / (1 - ((1 - e2) * z / R));
    }

    public double CalculateOpalUnZSlope(double r, double R, double e2, double H,
        double A3, double A4, double A5, double A6, double A7, double A8,
        double A9, double A10, double A11, double A12, double A13)
    {
        // First, compute z using your sag method.
        double z = CalculateOpalUnZSag(r, R, e2, H, A3, A4, A5, A6, A7, A8, A9, A10, A11, A12, A13);
        double c = 1.0 / R;

        // Compute dQ/dw explicitly.
        double w = z / H;
        double dQdw = 3 * A3 * Math.Pow(w, 2) + 4 * A4 * Math.Pow(w, 3)
                                              + 5 * A5 * Math.Pow(w, 4) + 6 * A6 * Math.Pow(w, 5)
                                              + 7 * A7 * Math.Pow(w, 6) + 8 * A8 * Math.Pow(w, 7)
                                              + 9 * A9 * Math.Pow(w, 8) + 10 * A10 * Math.Pow(w, 9)
                                              + 11 * A11 * Math.Pow(w, 10) + 12 * A12 * Math.Pow(w, 11)
                                              + 13 * A13 * Math.Pow(w, 12);

        // Apply chain rule: dQ/dz = dQ/dw * (1/H)
        double dQdz = dQdw / H;

        // Now, differentiate your implicit function F(z, r) = z - (c*(r^2 + (1-e2)*z^2)/2) - Q(z/H) = 0
        // Partial derivative with respect to z:
        double dFdz = 1 - c * (1 - e2) * z - dQdz;
        // Partial derivative with respect to r (only r^2 contributes):
        double dFdr = -c * r;

        // Therefore, by implicit differentiation: dz/dr = - (dF/dr) / (dF/dz)
        double dzdr = (-dFdr) / dFdz;

        return dzdr;
    }

    public double CalculatePolySlope(double r,
        double A1, double A2, double A3, double A4, double A5, double A6,
        double A7, double A8, double A9, double A10, double A11, double A12, double A13)
    {
        const double tolerance = 1e-12;
        const int maxIterations = 1000;
        double rSquared = r * r;
        double z = 1.0;
        int iteration = 0;

        while (iteration < maxIterations)
        {
            double Q = A13;
            Q = Q * z + A12;
            Q = Q * z + A11;
            Q = Q * z + A10;
            Q = Q * z + A9;
            Q = Q * z + A8;
            Q = Q * z + A7;
            Q = Q * z + A6;
            Q = Q * z + A5;
            Q = Q * z + A4;
            Q = Q * z + A3;
            Q = Q * z + A2;
            Q = Q * z + A1;

            double P = z * Q;
            double Q_deriv = A13 * 12;
            Q_deriv = Q_deriv * z + A12 * 11;
            Q_deriv = Q_deriv * z + A11 * 10;
            Q_deriv = Q_deriv * z + A10 * 9;
            Q_deriv = Q_deriv * z + A9 * 8;
            Q_deriv = Q_deriv * z + A8 * 7;
            Q_deriv = Q_deriv * z + A7 * 6;
            Q_deriv = Q_deriv * z + A6 * 5;
            Q_deriv = Q_deriv * z + A5 * 4;
            Q_deriv = Q_deriv * z + A4 * 3;
            Q_deriv = Q_deriv * z + A3 * 2;
            Q_deriv = Q_deriv * z + A2;
            double P_deriv = Q + z * Q_deriv;
            double delta = (P - rSquared) / P_deriv;
            double zNew = z - delta;
            if (Math.Abs(delta) < tolerance)
            {
                break;
            }
            z = zNew;
            iteration++;
        }

        if (iteration >= maxIterations)
        {
            Console.WriteLine("Warning: Maximum iterations reached without convergence for CalculatePolySlope.");
        }

        double slopeDenominator = A1 + 2 * A2 * z + 3 * A3 * z * z + 4 * A4 * z * z * z +
                                   5 * A5 * Math.Pow(z, 4) + 6 * A6 * Math.Pow(z, 5) +
                                   7 * A7 * Math.Pow(z, 6) + 8 * A8 * Math.Pow(z, 7) +
                                   9 * A9 * Math.Pow(z, 8) + 10 * A10 * Math.Pow(z, 9) +
                                   11 * A11 * Math.Pow(z, 10) + 12 * A12 * Math.Pow(z, 11) +
                                   13 * A13 * Math.Pow(z, 12);

        return (2 * r) / slopeDenominator;
    }



    public double CalculateBestFitSphereRadius3Points(double maxR, double zmax)
    {
        return (maxR * maxR) / (2 * zmax) + zmax / 2;
    }

    public (double R4, double zm, double rm, double g, double Lz) CalculateBestFitSphereRadius4Points(
        double minR, double maxR, double zmin, double zmax)
    {
        double Lr = maxR - minR;
        double Lz = zmax - zmin;
        double twoF = Math.Sqrt(Lz * Lz + Lr * Lr);
        double zm_val = (zmin + zmax) / 2;
        double rm_val = (minR + maxR) / 2;
        double g_val = twoF * rm_val / Math.Abs(Lz);
        double R4_val = Math.Sqrt(g_val * g_val + Math.Pow(twoF / 2, 2));
        return (R4_val, zm_val, rm_val, g_val, Lz);
    }

    public double CalculateAsphericityForR3(double r, double z, double R3, double R)
    {
        return Math.Sign(R) * (Math.Abs(R3) - Math.Sqrt(Math.Pow(R3 - z, 2) + r * r));
    }

    public double CalculateAsphericityForR4(double r, double z, double R4, double zm, double rm, double g, double Lz)
    {
        double signLz = Math.Sign(Lz);
        double z0 = zm + signLz * Math.Sqrt(g * g - rm * rm);
        return Math.Sign(z) * (R4 - Math.Sqrt(Math.Pow(z0 - z, 2) + r * r));
    }

   /* public List<double> CalculateSlope(List<(double r, double z)> points)
    {
        List<double> slopes = new List<double>();
        int n = points.Count;
        if (n < 2) return slopes;

        for (int i = 0; i < n; i++)
        {
            double dz;
            if (i == 0)
            {
                // One-sided difference (forward difference) for first point
                double dr1 = points[i + 1].r - points[i].r;
                double dr2 = points[i + 2].r - points[i + 1].r;
                double dz1 = (points[i + 1].z - points[i].z) / dr1;
                double dz2 = (points[i + 2].z - points[i + 1].z) / dr2;
                dz = dz1 + (dz1 - dz2) * dr1 / (dr1 + dr2);  // Extrapolated forward difference
            }
            else if (i == n - 1)
            {
                // One-sided difference (backward difference) for last point
                double dr1 = points[i].r - points[i - 1].r;
                double dr2 = points[i - 1].r - points[i - 2].r;
                double dz1 = (points[i].z - points[i - 1].z) / dr1;
                double dz2 = (points[i - 1].z - points[i - 2].z) / dr2;
                dz = dz1 + (dz1 - dz2) * dr1 / (dr1 + dr2);  // Extrapolated backward difference
            }
            else
            {
                // Central difference for interior points
                double dr = points[i + 1].r - points[i - 1].r;
                dz = (points[i + 1].z - points[i - 1].z) / dr;
            }
            slopes.Add(dz);
        }
        return slopes;
    }
   */

    public double CalculateAberrationOfNormals(double z, double r, double slope, double R)
    {
        if (slope == 0)
        {
            throw new DivideByZeroException("Slope cannot be zero when calculating aberration of normals.");
        }
        return z + (r / slope) - R;
    }

}