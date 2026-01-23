namespace UniFramework.Runtime
{
    public struct PlaySoundParams
    {
        /// <summary>
        /// 音量大小 (0.0 ~ 1.0)，默认 1.0。
        /// </summary>
        public float Volume;

        /// <summary>
        /// 是否循环播放音效，true 表示循环。
        /// </summary>
        public bool Loop;

        /// <summary>
        /// 渐入时间（秒）。如果大于 0，则音效会从 0 音量在指定时间内淡入到目标音量。
        /// </summary>
        public float FadeInSeconds;

        /// <summary>
        /// 2D/3D 混合比率 (0.0 ~ 1.0)。
        /// 0 表示完全 2D（不随位置变化），1 表示完全 3D（随位置变化）。
        /// 默认可设为 0。
        /// </summary>
        public float SpatialBlend;

        /// <summary>
        /// 获取一个 <see cref="PlaySoundParams"/> 的默认参数配置。
        /// </summary>
        public static PlaySoundParams Default => new PlaySoundParams
        {
            Volume = 1.0f,
            Loop = false,
            FadeInSeconds = 0f,
            SpatialBlend = 0f
        };
    }
}