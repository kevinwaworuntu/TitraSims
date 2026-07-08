namespace InteractionLogic
{
    /// <summary>
    /// Implemented by components that drive an Animator's playback over time
    /// (e.g. <see cref="PlayAnimByProgression"/>, <see cref="PlayAnimByTouch"/>),
    /// so other systems can stop/reset them without knowing the concrete type.
    /// </summary>
    public interface IAnimationPlaybackController
    {
        /// Immediately stops playback and resets progression back to the start.
        void StopPlayback();
    }
}
