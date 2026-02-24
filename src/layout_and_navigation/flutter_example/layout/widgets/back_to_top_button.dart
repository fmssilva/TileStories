import 'package:flutter/material.dart';

/// Simple back-to-top button for scrollable content.
///
/// STUDENT NOTE: Connected to LayoutManager's ScrollController to scroll to top.
///
/// USAGE: Add to Stack overlay on scrollable pages (done automatically by LayoutManager)
class BackToTopButton extends StatelessWidget {
  final VoidCallback onPressed;

  const BackToTopButton({super.key, required this.onPressed});

  @override
  Widget build(BuildContext context) {
    return FloatingActionButton.small(
      onPressed: () {
        onPressed();
      },
      tooltip: 'Back to top',
      child: const Icon(Icons.arrow_upward),
    );
  }
}
