using System.IO;
using Xunit;
using Xunit.Abstractions;
using Program = SaveLockscreenImage.SaveLockscreenImage;

namespace Test
{
    public class Test
    {
        private readonly ITestOutputHelper _output;

        public Test(ITestOutputHelper output)
        {
            _output = output;
        }

        [Fact]
        public void GetSourcePathTest()
        {
            var path = Program.GetSourcePath();
            Assert.False(string.IsNullOrEmpty(path));
            _output.WriteLine(path);
        }

        [Fact]
        public void GetDestinationPathTest()
        {
            var path = Program.GetDestinationPath();
            Assert.False(string.IsNullOrEmpty(path));
            Assert.True(Directory.Exists(path));
            _output.WriteLine(path);
        }

        [Theory]
        [InlineData(3)]
        [InlineData(5)]
        [InlineData(6)]
        public void MyFirstTheory(int value)
        {
            Assert.True(IsOdd(value));
        }

        private static bool IsOdd(int value)
        {
            return value % 2 == 1;
        }
    }
}
