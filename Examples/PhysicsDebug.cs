namespace EyE.Diagnostics.Examples
{
    class PhysicsDebug : CategoryLogBase<PhysicsDebug>
    {
        protected override string CategoryName => "Physics";
    }
    class PhysicsAssert : CategoryAssertBase<PhysicsAssert>
    {
        protected override string CategoryName => "Physics";
    }
}