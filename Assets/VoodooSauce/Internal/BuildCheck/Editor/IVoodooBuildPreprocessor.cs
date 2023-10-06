namespace Voodoo.Sauce.Internal.Editor
{
    /**
     * This interface is used if you want to add an additional build validation steps. Returning false in the build
     * method will cancel the whole build.
     */
    public interface IVoodooBuildPreprocessor {	
        bool Build();	
    }
}