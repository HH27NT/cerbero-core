namespace Cerbero.Core.Risk
{
    public readonly struct PositionSizeResult
    {
        public int Quantity { get; }
        public double TicksAtRisk { get; }
        public double DollarsAtRisk { get; }
        public double ActualRiskPercent { get; }

        public PositionSizeResult(int quantity, double ticksAtRisk, double dollarsAtRisk, double actualRiskPercent)
        {
            Quantity = quantity;
            TicksAtRisk = ticksAtRisk;
            DollarsAtRisk = dollarsAtRisk;
            ActualRiskPercent = actualRiskPercent;
        }
    }
}
