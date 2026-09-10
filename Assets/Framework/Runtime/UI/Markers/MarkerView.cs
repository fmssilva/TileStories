        private Vector2 _baseLabelAnchoredPosition;
                private bool _hasLabelPosition;
        private bool _hasLabelOffset;

        // Hide/show just this marker's label text (does not affect marker offset or
        // whole-marker visibility, which belongs to LODController per _2.5 Section 7).
        // Used by MarkerOverlapResolver's 2.5-h hide-fallback when displacement hits the
        // configured max and labels still crowd each other.
        public void SetLabelVisible(bool visible)
        {
            if (labelText != null)
                labelText.enabled = visible;
        }


        // Hide/show this marker's label text only (does not touch marker offset or
        // whole-marker visibility, which belongs to LODController per _2.5 Section 7).
        // Used by MarkerOverlapResolver's 2.5-h hide-fallback when displacement hits the
        // configured max and labels still crowd each other.
        public void SetLabelVisible(bool visible)
        {
            if (labelText != null)
                labelText.enabled = visible;
        }