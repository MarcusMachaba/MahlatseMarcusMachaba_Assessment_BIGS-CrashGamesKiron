namespace DatabaseLayer.Custom.Tests
{
    public class DBConnectionManagerTests
    {
        private const string TestConnString = @"Server=(localdb)\mssqllocaldb;Integrated Security=true;";

        [Fact]               
        public void CannotOpenMoreThan10Connections()
        {
            var conns = new ConnectionInfo[10];
            for (int i = 0; i < 10; i++)
            {
                conns[i] = new ConnectionInfo(TestConnString);
                conns[i].Open();
            }

            var extra = new ConnectionInfo(TestConnString);
            var ex = Assert.Throws<InvalidOperationException>(() => extra.Open());
            Assert.Contains("Maximum number of open database connections", ex.Message);

            foreach (var ci in conns) ci.Dispose();
            extra.Open();   // now succeeds
            extra.Dispose();
        }

        [Fact]
        public void ReleasesSlotWhenConnectionIsClosed()
        {
            var ci = new ConnectionInfo(TestConnString);
            ci.Open();
            ci.Dispose();

            for (int i = 0; i < 10; i++)
            {
                using var tmp = new ConnectionInfo(TestConnString);
                tmp.Open();  // should never throw
            }
        }
    }
}